const API_BASE_URL = 'http://localhost:5000/api';

function showTab(tabName) {
    document.querySelectorAll('.tab-content').forEach(tab => {
        tab.classList.remove('active');
    });
    document.querySelectorAll('.tab-button').forEach(btn => {
        btn.classList.remove('active');
    });
    
    document.getElementById(`${tabName}-tab`).classList.add('active');
    event.target.classList.add('active');
}

function generateUserId() {
    const userId = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        const r = Math.random() * 16 | 0;
        const v = c === 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
    document.getElementById('userId').value = userId;
    showMessage('Новый User ID сгенерирован', 'info');
}

function getUserId() {
    return document.getElementById('userId').value.trim();
}

function showMessage(text, type = 'info') {
    const messagesDiv = document.getElementById('messages');
    const messageDiv = document.createElement('div');
    messageDiv.className = `message ${type}`;
    messageDiv.textContent = text;
    messagesDiv.appendChild(messageDiv);
    
    setTimeout(() => {
        messageDiv.remove();
    }, 5000);
}

function displayResult(elementId, data, isError = false) {
    const element = document.getElementById(elementId);
    element.className = `result ${isError ? 'error' : 'success'}`;
    
    if (isError) {
        element.innerHTML = `<strong>Ошибка:</strong> ${data}`;
    } else if (typeof data === 'string') {
        element.innerHTML = data;
    } else {
        element.innerHTML = `<pre>${JSON.stringify(data, null, 2)}</pre>`;
    }
}

async function apiCall(endpoint, method = 'GET', body = null) {
    try {
        const options = {
            method,
            headers: {
                'Content-Type': 'application/json',
            }
        };
        
        if (body) {
            options.body = JSON.stringify(body);
        }
        
        const response = await fetch(`${API_BASE_URL}${endpoint}`, options);
        const data = await response.json();
        
        if (!response.ok) {
            throw new Error(data.error || `HTTP ${response.status}`);
        }
        
        return data;
    } catch (error) {
        throw error;
    }
}

async function createAccount() {
    const userId = getUserId();
    if (!userId) {
        showMessage('Введите User ID', 'error');
        return;
    }
    
    try {
        displayResult('createAccountResult', '<span class="loading"></span> Загрузка...', false);
        const result = await apiCall('/accounts', 'POST', { userId });
        displayResult('createAccountResult', `Счет успешно создан!<br>ID: ${result.id}<br>Баланс: ${result.balance} ₽`, false);
        showMessage('Счет создан успешно', 'success');
        getAccount();
    } catch (error) {
        displayResult('createAccountResult', error.message, true);
        showMessage(`Ошибка: ${error.message}`, 'error');
    }
}

async function topUpAccount() {
    const userId = getUserId();
    const amount = parseFloat(document.getElementById('topUpAmount').value);
    
    if (!userId) {
        showMessage('Введите User ID', 'error');
        return;
    }
    
    if (!amount || amount <= 0) {
        showMessage('Введите корректную сумму', 'error');
        return;
    }
    
    try {
        displayResult('topUpResult', '<span class="loading"></span> Загрузка...', false);
        const result = await apiCall(`/accounts/${userId}/topup`, 'POST', { amount });
        displayResult('topUpResult', `Счет пополнен!<br>Новый баланс: ${result.balance} ₽`, false);
        showMessage(`Счет пополнен на ${amount} ₽`, 'success');
        getAccount();
    } catch (error) {
        displayResult('topUpResult', error.message, true);
        showMessage(`Ошибка: ${error.message}`, 'error');
    }
}

async function getAccount() {
    const userId = getUserId();
    if (!userId) {
        showMessage('Введите User ID', 'error');
        return;
    }
    
    try {
        document.getElementById('accountInfo').innerHTML = '<span class="loading"></span> Загрузка...';
        const result = await apiCall(`/accounts/${userId}`, 'GET');
        
        const accountHtml = `
            <div class="account-info">
                <h4>Информация о счете</h4>
                <p><strong>ID счета:</strong> ${result.id}</p>
                <p><strong>User ID:</strong> ${result.userId}</p>
                <div class="balance">Баланс: ${result.balance} ₽</div>
                <p><strong>Создан:</strong> ${new Date(result.createdAt).toLocaleString('ru-RU')}</p>
            </div>
        `;
        
        document.getElementById('accountInfo').innerHTML = accountHtml;
        document.getElementById('accountInfo').className = 'result success';
    } catch (error) {
        displayResult('accountInfo', error.message, true);
    }
}

async function createOrder() {
    const userId = getUserId();
    const amount = parseFloat(document.getElementById('orderAmount').value);
    const description = document.getElementById('orderDescription').value.trim();
    
    if (!userId) {
        showMessage('Введите User ID', 'error');
        return;
    }
    
    if (!amount || amount <= 0) {
        showMessage('Введите корректную сумму', 'error');
        return;
    }
    
    if (!description) {
        showMessage('Введите описание заказа', 'error');
        return;
    }
    
    try {
        displayResult('createOrderResult', '<span class="loading"></span> Загрузка...', false);
        const result = await apiCall('/orders', 'POST', { userId, amount, description });
        displayResult('createOrderResult', `Заказ создан!<br>ID: ${result.id}<br>Статус: ${result.status}<br>Сумма: ${result.amount} ₽`, false);
        showMessage('Заказ создан успешно. Ожидайте обработки платежа...', 'success');
        setTimeout(() => getOrders(), 2000);
    } catch (error) {
        displayResult('createOrderResult', error.message, true);
        showMessage(`Ошибка: ${error.message}`, 'error');
    }
}

async function getOrders() {
    const userId = getUserId();
    if (!userId) {
        showMessage('Введите User ID', 'error');
        return;
    }
    
    try {
        document.getElementById('ordersList').innerHTML = '<span class="loading"></span> Загрузка...';
        const orders = await apiCall(`/orders/user/${userId}`, 'GET');
        
        if (!orders || orders.length === 0) {
            document.getElementById('ordersList').innerHTML = '<p>У вас пока нет заказов</p>';
            document.getElementById('ordersList').className = 'result info';
            return;
        }
        
        let ordersHtml = '';
        orders.forEach(order => {
            const statusClass = order.status.toLowerCase();
            const statusText = {
                'new': 'Новый',
                'finished': 'Завершен',
                'cancelled': 'Отменен'
            }[order.status.toLowerCase()] || order.status;
            
            ordersHtml += `
                <div class="order-item">
                    <h4>Заказ #${order.id.substring(0, 8)}...</h4>
                    <p><strong>Описание:</strong> ${order.description}</p>
                    <p><strong>Сумма:</strong> ${order.amount} ₽</p>
                    <p><strong>Статус:</strong> <span class="status-badge status-${statusClass}">${statusText}</span></p>
                    <p><strong>Создан:</strong> ${new Date(order.createdAt).toLocaleString('ru-RU')}</p>
                    <p><strong>Обновлен:</strong> ${new Date(order.updatedAt).toLocaleString('ru-RU')}</p>
                </div>
            `;
        });
        
        document.getElementById('ordersList').innerHTML = ordersHtml;
        document.getElementById('ordersList').className = 'result success';
    } catch (error) {
        displayResult('ordersList', error.message, true);
    }
}

document.addEventListener('DOMContentLoaded', () => {
    showMessage('Добро пожаловать в HSEGozon!', 'info');
});

