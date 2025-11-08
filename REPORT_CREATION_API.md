# API Создания Обращений - TazaOrda

## 📋 Обзор

Реализована полноценная система создания обращений с загрузкой фотографий через MinIO.

## 🔧 Предварительная настройка MinIO

### Установка MinIO (Docker)

```bash
docker run -d \
  -p 9000:9000 \
  -p 9001:9001 \
  --name minio \
  -e "MINIO_ROOT_USER=minioadmin" \
  -e "MINIO_ROOT_PASSWORD=minioadmin" \
  -v /tmp/minio/data:/data \
  quay.io/minio/minio server /data --console-address ":9001"
```

После запуска:
- MinIO API: http://localhost:9000
- MinIO Console: http://localhost:9001 (login: minioadmin / minioadmin)

### Конфигурация в appsettings.json

```json
{
  "Minio": {
    "Endpoint": "localhost:9000",
    "AccessKey": "minioadmin",
    "SecretKey": "minioadmin",
    "Bucket": "tazaorda",
    "UseSSL": "false",
    "PublicAccess": "true"
  }
}
```

---

## 📡 API Endpoints

### 1. Загрузка фото обращения

**POST /api/files/upload**

Загружает фото в MinIO и возвращает URL для использования в обращении.

**Требуется аутентификация**: Да

**Content-Type**: `multipart/form-data`

**Параметры:**
- `file` (form-data) - файл изображения
- `folder` (query, optional) - папка для сохранения (default: "reports")

**Ограничения:**
- Максимальный размер: 5 MB
- Разрешённые форматы: .jpg, .jpeg, .png, .gif, .webp
- MIME типы: image/jpeg, image/jpg, image/png, image/gif, image/webp

**Ответ:**
```json
{
  "url": "http://localhost:9000/tazaorda/reports/2025/01/08/3fa85f64-5717-4562-b3fc-2c963f66afa6.jpg",
  "path": "reports/2025/01/08/3fa85f64-5717-4562-b3fc-2c963f66afa6.jpg",
  "fileName": "3fa85f64-5717-4562-b3fc-2c963f66afa6.jpg",
  "fileSize": 245678,
  "contentType": "image/jpeg"
}
```

**Пример запроса (curl):**
```bash
curl -X POST http://localhost:5000/api/files/upload \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -F "file=@/path/to/photo.jpg" \
  -F "folder=reports"
```

**Пример запроса (JavaScript):**
```javascript
const formData = new FormData();
formData.append('file', fileInput.files[0]);

const response = await fetch('http://localhost:5000/api/files/upload', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${accessToken}`
  },
  body: formData
});

const result = await response.json();
console.log('File URL:', result.url);
```

---

### 2. Создание обращения

**POST /api/reports**

Создаёт новое обращение с использованием URL фотографии из MinIO.

**Требуется аутентификация**: Да

**Content-Type**: `application/json`

**Тело запроса:**
```json
{
  "category": "OverflowingBin",
  "description": "Возле дома №12 контейнер заполнен",
  "lat": 44.845,
  "lng": 65.507,
  "photoUrl": "http://localhost:9000/tazaorda/reports/2025/01/08/3fa85f64.jpg",
  "street": "ул. Ленина, 12",
  "districtId": "guid-optional"
}
```

**Поля:**
- `category` (string, required) - категория из списка категорий
- `description` (string, required) - описание проблемы
- `lat` (double, required) - широта
- `lng` (double, required) - долгота
- `photoUrl` (string, optional) - URL фото из MinIO
- `street` (string, optional) - адрес
- `districtId` (guid, optional) - ID района

**Ответ:**
```json
{
  "message": "Report created successfully",
  "reportId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Пример запроса (curl):**
```bash
curl -X POST http://localhost:5000/api/reports \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "category": "OverflowingBin",
    "description": "Переполненный контейнер",
    "lat": 44.845,
    "lng": 65.507,
    "photoUrl": "http://localhost:9000/tazaorda/reports/2025/01/08/photo.jpg"
  }'
```

---

### 3. Получение категорий обращений

**GET /api/categories**

Возвращает список всех доступных категорий для создания обращения.

**Требуется аутентификация**: Нет

**Ответ:**
```json
[
  {
    "id": 0,
    "name": "Переполненный бак",
    "description": "Контейнер для мусора переполнен",
    "iconUrl": "🗑️"
  },
  {
    "id": 1,
    "name": "Мусор на улице",
    "description": "Мусор в общественных местах",
    "iconUrl": "🧹"
  },
  {
    "id": 2,
    "name": "Нелегальная свалка",
    "description": "Несанкционированная свалка мусора",
    "iconUrl": "🚫"
  },
  {
    "id": 3,
    "name": "Не вывезен мусор",
    "description": "Мусор не был вывезен вовремя",
    "iconUrl": "🚛"
  },
  {
    "id": 4,
    "name": "Повреждённый контейнер",
    "description": "Контейнер повреждён или сломан",
    "iconUrl": "🔨"
  },
  {
    "id": 5,
    "name": "Неубранный снег/лёд",
    "description": "Необходима уборка снега или льда",
    "iconUrl": "❄️"
  },
  {
    "id": 6,
    "name": "Другое",
    "description": "Другие проблемы, связанные с отходами",
    "iconUrl": "❓"
  }
]
```

**Пример запроса (curl):**
```bash
curl -X GET http://localhost:5000/api/categories
```

---

### 4. Удаление файла

**DELETE /api/files/{*filePath}**

Удаляет файл из MinIO хранилища.

**Требуется аутентификация**: Да

**Параметры:**
- `filePath` - путь к файлу (например: reports/2025/01/08/photo.jpg)

**Ответ:**
```json
{
  "message": "Файл успешно удалён"
}
```

**Пример запроса (curl):**
```bash
curl -X DELETE http://localhost:5000/api/files/reports/2025/01/08/photo.jpg \
  -H "Authorization: Bearer YOUR_ACCESS_TOKEN"
```

---

## 🔄 Полный сценарий создания обращения

### Шаг 1: Получить категории

```javascript
const categoriesResponse = await fetch('http://localhost:5000/api/categories');
const categories = await categoriesResponse.json();
```

### Шаг 2: Загрузить фото

```javascript
const formData = new FormData();
formData.append('file', photoFile);

const uploadResponse = await fetch('http://localhost:5000/api/files/upload', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${accessToken}`
  },
  body: formData
});

const uploadResult = await uploadResponse.json();
const photoUrl = uploadResult.url;
```

### Шаг 3: Создать обращение

```javascript
const reportData = {
  category: 'OverflowingBin',
  description: 'Переполненный контейнер',
  lat: 44.845,
  lng: 65.507,
  photoUrl: photoUrl
};

const reportResponse = await fetch('http://localhost:5000/api/reports', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${accessToken}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify(reportData)
});

const reportResult = await reportResponse.json();
console.log('Report ID:', reportResult.reportId);
```

---

## ⚠️ Обработка ошибок

### Ошибки загрузки файла:

**400 Bad Request:**
```json
{
  "message": "Размер файла не должен превышать 5 MB"
}
```

```json
{
  "message": "Разрешены только следующие форматы: .jpg, .jpeg, .png, .gif, .webp"
}
```

**401 Unauthorized:**
```json
{
  "message": "Пользователь не аутентифицирован"
}
```

**500 Internal Server Error:**
```json
{
  "message": "Произошла ошибка при загрузке файла"
}
```

### Ошибки создания обращения:

**400 Bad Request:**
```json
{
  "message": "Неверная категория обращения: InvalidCategory"
}
```

```json
{
  "message": "Не найдено ни одного района в системе"
}
```

---

## 📝 Примеры использования

### Полный пример на React:

```jsx
import React, { useState, useEffect } from 'react';

function CreateReportForm() {
  const [categories, setCategories] = useState([]);
  const [selectedCategory, setSelectedCategory] = useState('');
  const [description, setDescription] = useState('');
  const [photo, setPhoto] = useState(null);
  const [photoUrl, setPhotoUrl] = useState('');
  const [location, setLocation] = useState({ lat: 44.845, lng: 65.507 });

  useEffect(() => {
    // Загрузить категории
    fetch('http://localhost:5000/api/categories')
      .then(res => res.json())
      .then(data => setCategories(data));
  }, []);

  const handlePhotoUpload = async (e) => {
    const file = e.target.files[0];
    if (!file) return;

    const formData = new FormData();
    formData.append('file', file);

    try {
      const response = await fetch('http://localhost:5000/api/files/upload', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`
        },
        body: formData
      });

      const result = await response.json();
      setPhotoUrl(result.url);
      setPhoto(file);
      alert('Фото загружено успешно!');
    } catch (error) {
      alert('Ошибка загрузки фото');
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    const reportData = {
      category: selectedCategory,
      description,
      lat: location.lat,
      lng: location.lng,
      photoUrl
    };

    try {
      const response = await fetch('http://localhost:5000/api/reports', {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('token')}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(reportData)
      });

      const result = await response.json();
      alert(`Обращение создано! ID: ${result.reportId}`);
    } catch (error) {
      alert('Ошибка создания обращения');
    }
  };

  return (
    <form onSubmit={handleSubmit}>
      <select value={selectedCategory} onChange={(e) => setSelectedCategory(e.target.value)}>
        <option value="">Выберите категорию</option>
        {categories.map(cat => (
          <option key={cat.id} value={Object.keys(cat)[0]}>
            {cat.iconUrl} {cat.name}
          </option>
        ))}
      </select>

      <textarea 
        value={description}
        onChange={(e) => setDescription(e.target.value)}
        placeholder="Описание проблемы"
      />

      <input 
        type="file" 
        accept="image/*"
        onChange={handlePhotoUpload}
      />

      {photoUrl && <img src={photoUrl} alt="Preview" width="200" />}

      <button type="submit">Создать обращение</button>
    </form>
  );
}
```

---

## ✅ Реализованные Функции

- ✅ Загрузка фото в MinIO с валидацией
- ✅ Автоматическая организация файлов по папкам (год/месяц/день)
- ✅ Генерация уникальных имён файлов (GUID)
- ✅ Публичный и presigned URL доступ
- ✅ Автоматическое создание bucket
- ✅ Установка политик доступа
- ✅ Создание обращений с фото
- ✅ Получение категорий обращений
- ✅ Удаление файлов
- ✅ Проверка существования файлов
- ✅ Ограничение размера файла (5 MB)
- ✅ Валидация форматов (jpg, jpeg, png, gif, webp)

---

**Разработано для проекта TazaOrda - Платформа управления чистотой города Кызылорда**
