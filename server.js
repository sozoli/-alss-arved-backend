const express = require('express');
const multer = require('multer');
const { Pool } = require('pg');
const path = require('path');
const fs = require('fs');
const cors = require('cors'); // Подключаем CORS

const app = express();
const upload = multer({ dest: 'uploads/' });  // Папка для хранения файлов

// Создаем папку uploads, если она не существует
const uploadsDir = path.join(__dirname, 'uploads');
// Разрешаем все домены для CORS (можно ограничить конкретным доменом)
app.use(cors({
    origin: 'https://alss.itb2203.tautar.ee', // Разрешаем только этот домен
  }));

// Настройка для отдачи статических файлов из папки 'uploads'
app.use('/uploads', express.static(path.join(__dirname, 'uploads')));

// Настройка подключения к базе данных PostgreSQL
const pool = new Pool({
  user: 'postgres',
  host: 'localhost',
  database: 'my-db',
  password: 'postgres',
  port: 5432,
});

// Обработчик для загрузки аватара
app.put('/api/auth/users/:id/avatar', upload.single('avatar'), async (req, res) => {
  const file = req.file;  // Получаем файл из запроса

  if (!file) {
    return res.status(400).send('No file uploaded');
  }

  try {
    // Генерируем путь для сохранения файла
    const avatarPath = `/uploads/${file.filename}`;

    // Обновляем аватар в базе данных
    const result = await pool.query(
      'UPDATE users SET avatarUrl = $1 WHERE id = $2 RETURNING avatarUrl',
      [avatarPath, req.params.id]
    );

    // Отправляем успешный ответ с аватаром
    res.status(200).json({ avatarUrl: avatarPath });
  } catch (error) {
    console.error('Error uploading avatar:', error);
    res.status(500).send('Error uploading avatar');
  }
});

// Запускаем сервер
const PORT = process.env.PORT || 5000;
app.listen(PORT, () => {
  console.log(`Server is running on port ${PORT}`);
});