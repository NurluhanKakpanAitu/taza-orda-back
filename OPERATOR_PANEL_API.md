# API Операторской Панели - TazaOrda

## 📋 Обзор

Реализована полноценная система управления обращениями для операторов и администраторов платформы TazaOrda.

## 🔐 Авторизация и Роли

Все эндпоинты операторской панели требуют:
- **Аутентификацию**: Bearer токен в заголовке `Authorization`
- **Права доступа**: Роль `Operator` или `Admin`

При попытке доступа без прав возвращается:
- **401 Unauthorized** - если токен отсутствует или недействителен
- **403 Forbidden** - если у пользователя нет нужной роли

---

## 📡 API Endpoints

### 1. Получить список обращений с фильтрацией

**GET /api/operator/reports**

Получить пагинированный список обращений с возможностью фильтрации.

**Параметры запроса (Query Parameters):**
- `status` (optional) - фильтр по статусу (New, InProgress, Completed, Rejected, UnderReview, Closed)
- `district_id` (optional, guid) - фильтр по району
- `category_id` (optional) - фильтр по категории обращения
- `from` (optional, datetime) - начало периода
- `to` (optional, datetime) - конец периода
- `page` (optional, default: 1) - номер страницы
- `size` (optional, default: 20) - размер страницы (макс 100)
- `search` (optional) - поиск по описанию, адресу, имени пользователя

**Ответ:**
```json
{
  "reports": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "category": "OverflowingBin",
      "categoryName": "Переполненный бак",
      "status": "New",
      "description": "Контейнер переполнен возле дома №12",
      "latitude": 44.845,
      "longitude": 65.507,
      "street": "ул. Ленина, 12",
      "createdAt": "2025-01-08T10:30:00Z",
      "userName": "Алия Нурлыхан",
      "districtName": "Кызылжарма",
      "hasPhotoBefore": true,
      "hasPhotoAfter": false
    }
  ],
  "total": 150,
  "page": 1,
  "size": 20,
  "totalPages": 8
}
```

**Пример запроса:**
```bash
curl -X GET "http://localhost:5000/api/operator/reports?status=New&page=1&size=20" \
  -H "Authorization: Bearer YOUR_OPERATOR_TOKEN"
```

---

### 2. Получить детали обращения

**GET /api/operator/reports/{id}**

Получить полную информацию об конкретном обращении.

**Параметры:**
- `id` (guid, path) - ID обращения

**Ответ:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "category": "OverflowingBin",
  "categoryName": "Переполненный бак",
  "status": "InProgress",
  "description": "Контейнер переполнен возле дома №12",
  "latitude": 44.845,
  "longitude": 65.507,
  "street": "ул. Ленина, 12",
  "photoBefore": "http://localhost:9000/tazaorda/reports/2025/01/08/photo123.jpg",
  "photoAfter": null,
  "createdAt": "2025-01-08T10:30:00Z",
  "updatedAt": "2025-01-08T11:00:00Z",
  "completedAt": null,
  "operatorComment": null,
  "rating": null,
  "userFeedback": null,
  "userId": "user-guid-123",
  "userName": "Алия Нурлыхан",
  "userPhone": "+77012345678",
  "districtId": "district-guid-456",
  "districtName": "Кызылжарма",
  "assignedOperatorId": "operator-guid-789",
  "assignedOperatorName": "Асет Ибрагимов"
}
```

**Пример запроса:**
```bash
curl -X GET "http://localhost:5000/api/operator/reports/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
  -H "Authorization: Bearer YOUR_OPERATOR_TOKEN"
```

---

### 3. Изменить статус обращения

**PATCH /api/operator/reports/{id}/status**

Изменить статус обращения с возможностью добавления комментария.

**Параметры:**
- `id` (guid, path) - ID обращения

**Тело запроса:**
```json
{
  "status": "Completed",
  "operatorComment": "Контейнер очищен, проблема решена"
}
```

**Поля:**
- `status` (required) - новый статус (New, InProgress, Completed, Rejected, UnderReview, Closed)
- `operatorComment` (optional) - комментарий оператора

**Ответ:**
```json
{
  "message": "Статус обращения успешно обновлён"
}
```

**Логика:**
- При изменении статуса автоматически создаётся запись в AuditLog
- Если новый статус `Completed` и дата завершения ещё не установлена:
  - Устанавливается `completedAt = now`
  - Пользователю начисляется 10 coins
  - Создаётся транзакция CoinTransaction
- Если оператор ещё не назначен, автоматически назначается текущий оператор

**Пример запроса:**
```bash
curl -X PATCH "http://localhost:5000/api/operator/reports/3fa85f64-5717-4562-b3fc-2c963f66afa6/status" \
  -H "Authorization: Bearer YOUR_OPERATOR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "status": "Completed",
    "operatorComment": "Проблема решена"
  }'
```

---

### 4. Загрузить фото "после"

**POST /api/operator/reports/{id}/photo-after**

Загрузить фотографию результата выполнения работ.

**Параметры:**
- `id` (guid, path) - ID обращения

**Content-Type:** `multipart/form-data`

**Поля:**
- `file` (form-data) - файл изображения

**Ограничения:**
- Максимальный размер: 5 MB
- Разрешённые форматы: .jpg, .jpeg, .png, .gif, .webp

**Ответ:**
```json
{
  "message": "Фото успешно загружено",
  "photoUrl": "http://localhost:9000/tazaorda/reports/after/2025/01/08/photo456.jpg"
}
```

**Логика:**
- Фото загружается в MinIO в папку `reports/after`
- URL сохраняется в базе данных (когда будет добавлено поле PhotoAfter)
- Если обращение не найдено, загруженный файл автоматически удаляется

**Пример запроса:**
```bash
curl -X POST "http://localhost:5000/api/operator/reports/3fa85f64-5717-4562-b3fc-2c963f66afa6/photo-after" \
  -H "Authorization: Bearer YOUR_OPERATOR_TOKEN" \
  -F "file=@/path/to/photo_after.jpg"
```

---

### 5. Получить статистику

**GET /api/operator/stats**

Получить статистику обращений за период.

**Параметры:**
- `period` (optional, default: "today") - период статистики
  - `today` - сегодня
  - `week` - последние 7 дней
  - `month` - последние 30 дней
  - `year` - последние 365 дней

**Ответ:**
```json
{
  "total": 150,
  "new": 25,
  "inProgress": 45,
  "done": 70,
  "cancelled": 10,
  "period": "today",
  "fromDate": "2025-01-08T00:00:00Z",
  "toDate": "2025-01-08T23:59:59Z"
}
```

**Пример запроса:**
```bash
curl -X GET "http://localhost:5000/api/operator/stats?period=week" \
  -H "Authorization: Bearer YOUR_OPERATOR_TOKEN"
```

---

### 6. Назначить обращение на себя

**POST /api/operator/reports/{id}/assign**

Назначить обращение на текущего оператора.

**Параметры:**
- `id` (guid, path) - ID обращения

**Ответ:**
```json
{
  "message": "Обращение успешно назначено на вас"
}
```

**Логика:**
- Обращение назначается на текущего оператора (из токена)
- Если статус обращения `New`, автоматически меняется на `InProgress`
- Обновляется `updatedAt`

**Пример запроса:**
```bash
curl -X POST "http://localhost:5000/api/operator/reports/3fa85f64-5717-4562-b3fc-2c963f66afa6/assign" \
  -H "Authorization: Bearer YOUR_OPERATOR_TOKEN"
```

---

## 🔄 Жизненный цикл обращения

### Статусы обращения:

1. **New** (Новое)
   - Только что создано пользователем
   - Ожидает рассмотрения оператором

2. **InProgress** (В работе)
   - Назначен оператор
   - Работа ведётся

3. **UnderReview** (На проверке)
   - Работа выполнена
   - Ожидает подтверждения/проверки

4. **Completed** (Выполнено)
   - Проблема решена
   - Пользователь получает 10 coins
   - Может оставить отзыв

5. **Rejected** (Отклонено)
   - Обращение признано недействительным
   - Дубликат или некорректное

6. **Closed** (Закрыто)
   - Обращение закрыто с подтверждением

### Рекомендуемый flow оператора:

```
1. GET /operator/reports?status=New
   ↓
2. GET /operator/reports/{id}  (просмотр деталей)
   ↓
3. POST /operator/reports/{id}/assign  (назначить на себя)
   ↓ статус автоматически → InProgress
4. PATCH /operator/reports/{id}/status {status: "UnderReview"}
   ↓
5. POST /operator/reports/{id}/photo-after  (загрузить фото результата)
   ↓
6. PATCH /operator/reports/{id}/status {status: "Completed", comment: "Выполнено"}
   ↓ пользователь получает 10 coins
```

---

## 🎯 Примеры использования

### Пример 1: Обработка нового обращения

```javascript
// 1. Получить список новых обращений
const newReports = await fetch('http://localhost:5000/api/operator/reports?status=New', {
  headers: {
    'Authorization': `Bearer ${operatorToken}`
  }
}).then(r => r.json());

// 2. Выбрать обращение и посмотреть детали
const reportDetails = await fetch(`http://localhost:5000/api/operator/reports/${reportId}`, {
  headers: {
    'Authorization': `Bearer ${operatorToken}`
  }
}).then(r => r.json());

// 3. Назначить на себя
await fetch(`http://localhost:5000/api/operator/reports/${reportId}/assign`, {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${operatorToken}`
  }
});

// 4. После выполнения работ - изменить статус
await fetch(`http://localhost:5000/api/operator/reports/${reportId}/status`, {
  method: 'PATCH',
  headers: {
    'Authorization': `Bearer ${operatorToken}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    status: 'Completed',
    operatorComment: 'Контейнер очищен'
  })
});
```

### Пример 2: Получение статистики для дашборда

```javascript
const getOperatorDashboard = async () => {
  // Статистика за сегодня
  const todayStats = await fetch('http://localhost:5000/api/operator/stats?period=today', {
    headers: { 'Authorization': `Bearer ${operatorToken}` }
  }).then(r => r.json());

  // Статистика за неделю
  const weekStats = await fetch('http://localhost:5000/api/operator/stats?period=week', {
    headers: { 'Authorization': `Bearer ${operatorToken}` }
  }).then(r => r.json());

  // Активные обращения
  const activeReports = await fetch('http://localhost:5000/api/operator/reports?status=InProgress', {
    headers: { 'Authorization': `Bearer ${operatorToken}` }
  }).then(r => r.json());

  return {
    today: todayStats,
    week: weekStats,
    active: activeReports
  };
};
```

### Пример 3: React компонент операторской панели

```jsx
import React, { useState, useEffect } from 'react';

function OperatorDashboard() {
  const [stats, setStats] = useState(null);
  const [reports, setReports] = useState([]);
  const [filter, setFilter] = useState({ status: 'New', page: 1 });

  useEffect(() => {
    loadDashboard();
  }, [filter]);

  const loadDashboard = async () => {
    const token = localStorage.getItem('operatorToken');

    // Загрузка статистики
    const statsRes = await fetch('http://localhost:5000/api/operator/stats?period=today', {
      headers: { 'Authorization': `Bearer ${token}` }
    });
    setStats(await statsRes.json());

    // Загрузка обращений
    const reportsRes = await fetch(
      `http://localhost:5000/api/operator/reports?status=${filter.status}&page=${filter.page}`,
      { headers: { 'Authorization': `Bearer ${token}` } }
    );
    const reportsData = await reportsRes.json();
    setReports(reportsData.reports);
  };

  const handleAssign = async (reportId) => {
    const token = localStorage.getItem('operatorToken');
    
    await fetch(`http://localhost:5000/api/operator/reports/${reportId}/assign`, {
      method: 'POST',
      headers: { 'Authorization': `Bearer ${token}` }
    });

    alert('Обращение назначено на вас');
    loadDashboard();
  };

  const handleStatusChange = async (reportId, newStatus, comment) => {
    const token = localStorage.getItem('operatorToken');

    await fetch(`http://localhost:5000/api/operator/reports/${reportId}/status`, {
      method: 'PATCH',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ status: newStatus, operatorComment: comment })
    });

    alert('Статус обновлён');
    loadDashboard();
  };

  return (
    <div className="operator-dashboard">
      <h1>Панель оператора</h1>

      {/* Статистика */}
      {stats && (
        <div className="stats">
          <div className="stat-card">
            <h3>Всего</h3>
            <p>{stats.total}</p>
          </div>
          <div className="stat-card">
            <h3>Новые</h3>
            <p>{stats.new}</p>
          </div>
          <div className="stat-card">
            <h3>В работе</h3>
            <p>{stats.inProgress}</p>
          </div>
          <div className="stat-card">
            <h3>Выполнено</h3>
            <p>{stats.done}</p>
          </div>
        </div>
      )}

      {/* Фильтр */}
      <div className="filters">
        <select value={filter.status} onChange={(e) => setFilter({...filter, status: e.target.value})}>
          <option value="New">Новые</option>
          <option value="InProgress">В работе</option>
          <option value="Completed">Выполненные</option>
        </select>
      </div>

      {/* Список обращений */}
      <div className="reports-list">
        {reports.map(report => (
          <div key={report.id} className="report-card">
            <h3>{report.categoryName}</h3>
            <p>{report.description}</p>
            <p>Пользователь: {report.userName}</p>
            <p>Адрес: {report.street}</p>
            <p>Дата: {new Date(report.createdAt).toLocaleString()}</p>

            <div className="actions">
              <button onClick={() => handleAssign(report.id)}>
                Назначить на себя
              </button>
              <button onClick={() => handleStatusChange(report.id, 'Completed', 'Выполнено')}>
                Завершить
              </button>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
```

---

## ⚙️ Автоматические действия системы

### При смене статуса на Completed:

1. ✅ Устанавливается `completedAt = DateTime.UtcNow`
2. ✅ Пользователю начисляется **10 coins**
3. ✅ Создаётся запись в `CoinTransactions`:
   ```
   Type: Credit
   Reason: ReportCompleted
   Amount: 10
   Description: "Награда за выполненное обращение"
   ```
4. ✅ Создаётся запись в `AuditLog`:
   ```
   Action: Updated
   EntityType: Report
   OldValues: {"Status": "InProgress"}
   NewValues: {"Status": "Completed", "Comment": "..."}
   ```

### При назначении оператора:

1. ✅ Если статус `New` → автоматически меняется на `InProgress`
2. ✅ Устанавливается `assignedToId = operatorId`
3. ✅ Обновляется `updatedAt`

---

## ✅ Реализованные функции

- ✅ Проверка ролей через `RequireRoleAttribute`
- ✅ Пагинированный список обращений с фильтрацией
- ✅ Детальная информация об обращении
- ✅ Изменение статуса с комментарием
- ✅ Загрузка фото "после" в MinIO
- ✅ Статистика за различные периоды
- ✅ Назначение обращения на оператора
- ✅ Автоматическое начисление coins
- ✅ Аудит всех действий в AuditLog
- ✅ Поддержка поиска по тексту

---

**Разработано для проекта TazaOrda - Платформа управления чистотой города Кызылорда**
