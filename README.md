# WebRabbit-DSR-
Здесь представлен сервис для отправки и получения команд для клиентских устройств при помощи HTTP-запросов.
Также он предоставляет возможность просмотра истории команд по каждому устройству отдельно при помощи БД MongoDB.
Большая функциональная часть сервиса связана с отправкой и получением команд из очереди, при этом происходит проверка команды на время и, если она просрочена, то команда не поступает на выполнение и уничтожается. В случае отсутствия команд, организуется long-polling соединение до получения команды или до истечения timeout. В случае истечении времени, происходит генерация оишбки 426 и сервер ждет нового соединения от устройства.

Также возможна ошибка такого рода: при первом соединении клиентского устройства и если еще не была отправлена ни одна команда для этого устройства после запуска сервера RabbitMQ, также возвращается ошибка 426. Решается она путем отправки любой команды на это устройство.

Запрос на отправку команды представлен POST-запросом с телом:
{
deviceId:12345,
command: {
  "commandName": "setOnOff",
  "parameters":[
      {
        "name":"switchOn",
        "value":"true"
      }
    ]
}
}
Будьте внимательны к знакам пунктуации.
Формат команды: http://localhost:54863/commands

Для получения команды для конкретного устройства:
Формат команды: http://localhost:54863/commands/3/1, возможен другой вариант: http://localhost:54863/commands?deviceId=3&timeout=1, таймаут в секундах.

Для получения истории по устройству:
Формат команды: http://localhost:54863/history/1 или http://localhost:54863/history/deviceId=1

ВНИМАНИЕ: Для нормального функционирования необходим рабочий сервер БД MongoDB, в противном случае будет возвращаться внутренняя ошибка сервера.
Также для подключения к серверам RabbitMQ необходимо указать конфигурационные настройки подключения: порт, имя пользователя, пароль, хост, виртуальный хост.
Для MongoDb необходим порт, имя базы данных, строка подключения.

