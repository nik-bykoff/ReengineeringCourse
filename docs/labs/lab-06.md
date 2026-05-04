# Лабораторна робота 6. Безпечний рефакторинг EchoServer

**Дисципліна**: Реінжиніринг програмного забезпечення  
**Студент**: Биков Нікіта Вячеславович  
**Група**: ПЗС-1  
**Гілка**: `lab-06-echoserver-refactor`  
**Pull Request**: створюється з `nik-bykoff:lab-06-echoserver-refactor` у `lenagrin/ReengineeringCourse:master`

## Мета

Зробити проєкт `EchoTcpServer` придатним до тестування юніт-тестами, не змінюючи його зовнішню поведінку (контракт CLI, формат UDP-пакетів). Покрити нові та витягнуті частини тестами, заміряти приріст coverage.

## Хід виконання

### 1. Розщеплення єдиного `Program.cs`

Початковий файл [`EchoTcpServer/Program.cs`](https://github.com/lenagrin/ReengineeringCourse/blob/master/EchoTcpServer/Program.cs) (170+ рядків, три класи + точка входу) розщеплено на:

- [`EchoCore.cs`](../../EchoTcpServer/EchoCore.cs) — `public static class EchoCore.EchoLoopAsync(Stream, CancellationToken, int bufferSize)`. Це чистий, не залежний від сокетів алгоритм відлуння. Тут і живе тестована логіка.
- [`IEchoServer.cs`](../../EchoTcpServer/IEchoServer.cs) — мінімальний контракт `StartAsync()` / `Stop()`.
- [`EchoServer.cs`](../../EchoTcpServer/EchoServer.cs) — реалізує `IEchoServer`, маршалить життєвий цикл `TcpListener` і делегує тіло циклу у `EchoCore.EchoLoopAsync`.
- [`UdpTimedSender.cs`](../../EchoTcpServer/UdpTimedSender.cs) — окремий клас із `IsRunning`, валідацією `intervalMilliseconds`, перевіркою `null` для `host`, новим `internal` конструктором для майбутнього мокінгу `UdpClient`.
- [`Program.cs`](../../EchoTcpServer/Program.cs) — лише `Main`, тільки композиція.

Усі типи перенесено у namespace `EchoTcpServer` (раніше були у глобальному).

### 2. Зміни поведінки, які варто зафіксувати

- `EchoServer` тепер `sealed`; його `Stop()` ідемпотентний (повторний виклик не кидає `ObjectDisposedException`).
- `EchoServer.HandleClientAsync` стає приватним статичним методом, що делегує `EchoCore.EchoLoopAsync`. Поведінка ідентична попередній версії (та сама `byte[8192]` буфер за замовчуванням, аналогічна обробка `OperationCanceledException`).
- `UdpTimedSender` валідує аргументи (`null host`, `interval <= 0`), додано флаг `IsRunning` для прозорого життєвого циклу.

### 3. Новий проєкт тестів `EchoTcpServer.Tests`

Структура:

```text
EchoTcpServer.Tests/
  EchoTcpServer.Tests.csproj  (NUnit + Moq + coverlet.msbuild)
  EchoCoreTests.cs            (5 тестів)
  UdpTimedSenderTests.cs      (5 тестів)
```

Тести (10 шт., усі зелені):

| Файл | Тест | Що перевіряє |
|------|------|-------------|
| `EchoCoreTests` | `EchoLoopAsync_OnPlainPayload_WritesIdenticalBytesBack` | базовий echo через `BridgeStream` (in-memory) |
| `EchoCoreTests` | `EchoLoopAsync_StopsWhenStreamClosed` | завершення на EOS |
| `EchoCoreTests` | `EchoLoopAsync_RespectsCancellationToken` | швидке завершення на cancel |
| `EchoCoreTests` | `EchoLoopAsync_NullStream_Throws` | guard-clause для `null` |
| `EchoCoreTests` | `EchoLoopAsync_NonPositiveBufferSize_Throws` | guard-clause для `bufferSize <= 0` |
| `UdpTimedSenderTests` | `StartSending_TwiceWithoutStop_Throws` | вимога ексклюзивної роботи timer-а |
| `UdpTimedSenderTests` | `StopSending_AfterStart_AllowsSubsequentStart` | повторний старт після стопу |
| `UdpTimedSenderTests` | `StartSending_WithNonPositiveInterval_ThrowsArgumentOutOfRange` | валідація |
| `UdpTimedSenderTests` | `Constructor_NullHost_Throws` | валідація `null` |
| `UdpTimedSenderTests` | `IsRunning_ReflectsTimerLifecycle` | публічний індикатор стану |

Хелпер `BridgeStream` усередині `EchoCoreTests.cs` дозволяє підставляти `MemoryStream` як вхід та інший `MemoryStream` як вихід, що покриває echo-логіку без жодного TCP-сокета.

## Зміни у коді та конфігурації

| Файл | Зміна |
|------|-------|
| [`EchoTcpServer/EchoCore.cs`](../../EchoTcpServer/EchoCore.cs) | новий — testable echo loop |
| [`EchoTcpServer/IEchoServer.cs`](../../EchoTcpServer/IEchoServer.cs) | новий — інтерфейс |
| [`EchoTcpServer/EchoServer.cs`](../../EchoTcpServer/EchoServer.cs) | новий — клас сервера, sealed, реалізує `IEchoServer` |
| [`EchoTcpServer/UdpTimedSender.cs`](../../EchoTcpServer/UdpTimedSender.cs) | новий — у власному файлі, з валідацією, `IsRunning`, namespace |
| [`EchoTcpServer/Program.cs`](../../EchoTcpServer/Program.cs) | переписано — тільки `Main`/композиція |
| [`EchoTcpServer.Tests/`](../../EchoTcpServer.Tests/) | новий проєкт NUnit + Moq + coverlet.msbuild |
| [`NetSdrClient.sln`](../../NetSdrClient.sln) | + `EchoTcpServer.Tests` |

## Як перевірити

```bash
dotnet build NetSdrClient.sln -c Release
dotnet test NetSdrClient.sln -c Release

# З покриттям тільки для EchoServer
cd EchoTcpServer.Tests
dotnet test -c Release \
  /p:CollectCoverage=true \
  /p:CoverletOutput=TestResults/coverage.xml \
  /p:CoverletOutputFormat=opencover \
  /p:Exclude="[EchoServer]Program"
```

Очікуваний результат:

```text
Passed!  - Failed: 0, Passed: 18, ... NetSdrClientAppTests.dll
Passed!  - Failed: 0, Passed:  4, ... NetSdrClient.ArchTests.dll
Passed!  - Failed: 0, Passed: 10, ... EchoTcpServer.Tests.dll

+------------+--------+--------+--------+
| Module     | Line   | Branch | Method |
+------------+--------+--------+--------+
| EchoServer | 55.12% | 65.38% | 61.53% |
+------------+--------+--------+--------+
```

## Метрики до/після

| Метрика | До | Після |
|---------|----|-------|
| Кількість файлів у `EchoTcpServer/` | 1 | 5 |
| Кількість класів у `Program.cs` | 3 | 0 (тільки `Program`) |
| Тести проєкту `EchoServer` | 0 | 10 |
| Compiler warnings (`EchoTcpServer`) | 5 | 0 |
| Line coverage `EchoServer` модуля | 0% | 55.12% |
| Branch coverage | 0% | 65.38% |
| Method coverage | 0% | 61.53% |

Залишок незакритого coverage — це сокетні гілки `EchoServer.StartAsync`/`HandleClientAsync` (потребують live-listener, який не входить в обсяг unit-тестів) та exception-гілки `UdpTimedSender.SendMessageCallback` (опційно покриваються інтеграційним тестом).

## Висновки

Витягання `EchoCore` як чистого алгоритму над `Stream` — стандартний приклад «зробити код тестованим без зміни поведінки»: реальний код `EchoServer` тонко делегує туди логіку, а тести працюють із in-memory `MemoryStream`. У результаті `EchoTcpServer` зник з переліку «нульове покриття» і має чистий warnings-free бекграунд.

## Посилання

- Martin Fowler — *Refactoring: Improving the Design of Existing Code* (Extract Method, Replace Constructor with Factory).
- [.NET docs — `Stream` testing patterns](https://learn.microsoft.com/dotnet/standard/io/handling-io-errors)

## Скріни

```text
[ScreenSonar11] Sonar Coverage до — EchoServer 0%
[ScreenSonar12] Sonar Coverage після — EchoServer ~55%
```
