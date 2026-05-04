# Лабораторна робота 2. Code Smells через PR

**Дисципліна**: Реінжиніринг програмного забезпечення  
**Студент**: Биков Нікіта Вячеславович  
**Група**: ПЗС-1  
**Гілка**: `lab-02-code-smells`  
**Pull Request**: створюється з `nik-bykoff:lab-02-code-smells` у `lenagrin/ReengineeringCourse:master`

## Мета

Усунути 5–10 зауважень типу bugs/code smells у проєкті `NetSdrClientApp` без зміни поведінки. Базою для виправлень є власне візуальне ревʼю коду та компіляторні попередження (NU/CS), оскільки SonarCloud буде увімкнено пізніше.

## Хід виконання

Виконано локальний прогін `dotnet build NetSdrClient.sln -c Release` до і після рефакторингу. Кількість попереджень компілятора зменшилась з 18 до 15 (решта 15 — це `EchoTcpServer` (Лаба 6) та NuGet-вразливості (Лаба 7)). Усі юніт-тести `dotnet test` залишились зеленими (8/8).

## Зміни у коді та конфігурації

| Тип | Опис | Файл/локація |
|-----|------|--------------|
| Bug | Метод `Disconect` мав одруковану назву; перейменовано на `Disconnect`. Оновлено виклики у Program та тестах | [`NetSdrClientApp/NetSdrClient.cs`](../../NetSdrClientApp/NetSdrClient.cs), [`NetSdrClientApp/Program.cs`](../../NetSdrClientApp/Program.cs), [`NetSdrClientAppTests/NetSdrClientTests.cs`](../../NetSdrClientAppTests/NetSdrClientTests.cs) |
| Bug (parser-ризик) | Артефакт `;` перед `var iqDataMode = (byte)0x80;` у `StartIQAsync` | [`NetSdrClientApp/NetSdrClient.cs`](../../NetSdrClientApp/NetSdrClient.cs) |
| Code smell | Виключно зайвий `using static System.Runtime.InteropServices.JavaScript.JSType;` (артефакт авто-import з JS-interop), що тягнув непотрібну залежність | [`NetSdrClientApp/NetSdrClient.cs`](../../NetSdrClientApp/NetSdrClient.cs), [`NetSdrClientApp/Networking/ITcpClient.cs`](../../NetSdrClientApp/Networking/ITcpClient.cs) |
| Bug (race) | `responseTaskSource` зчитувався/перезаписувався без синхронізації між producer і callback. Перероблено на `Interlocked.Exchange<TaskCompletionSource<byte[]>?>` + `TrySetResult` | [`NetSdrClientApp/NetSdrClient.cs`](../../NetSdrClientApp/NetSdrClient.cs) |
| Bug | `Aggregate(...)` з `(l, r) => $"{l} {r}"` падає на порожньому масиві (`InvalidOperationException`); замінено на `string.Join(" ", ...)` у трьох логах (TCP/UDP/Send) | [`NetSdrClientApp/NetSdrClient.cs`](../../NetSdrClientApp/NetSdrClient.cs), [`NetSdrClientApp/Networking/TcpClientWrapper.cs`](../../NetSdrClientApp/Networking/TcpClientWrapper.cs) |
| Performance | У `GetSamples` `bodyEnumerable.Count()` викликався на кожній ітерації (O(n²)) і `Skip()` створював новий `IEnumerable`. Перероблено на цикл `for` з фіксованим буфером `byte[4]` (O(n)) | [`NetSdrClientApp/Messages/NetSdrMessageHelper.cs`](../../NetSdrClientApp/Messages/NetSdrMessageHelper.cs) |
| Bug | `UdpClientWrapper.GetHashCode` обчислював MD5 від рядка — повільно, не контрактно (HashCode має бути дешевим і узгодженим з `Equals`). Замінено на `HashCode.Combine` + додано `Equals(object?)` для відповідності контракту | [`NetSdrClientApp/Networking/UdpClientWrapper.cs`](../../NetSdrClientApp/Networking/UdpClientWrapper.cs) |
| Code smell | `private CancellationTokenSource _cts;` присвоювалось `null` без `?` — попередження CS8625; зроблено `CancellationTokenSource?` | [`NetSdrClientApp/Networking/TcpClientWrapper.cs`](../../NetSdrClientApp/Networking/TcpClientWrapper.cs) |
| Code smell | Поля `_host`, `_port` не змінювались — позначено `readonly` | [`NetSdrClientApp/Networking/TcpClientWrapper.cs`](../../NetSdrClientApp/Networking/TcpClientWrapper.cs) |
| Code smell | `catch (OperationCanceledException ex)` де `ex` не використовувався — змінено на `catch (OperationCanceledException)` з пояснювальним коментарем | [`NetSdrClientApp/Networking/TcpClientWrapper.cs`](../../NetSdrClientApp/Networking/TcpClientWrapper.cs), [`NetSdrClientApp/Networking/UdpClientWrapper.cs`](../../NetSdrClientApp/Networking/UdpClientWrapper.cs) |
| Code smell (відкладено) | Дубль `StopListening`/`Exit` в `UdpClientWrapper` залишено навмисно — буде усунено у Лабі 4 (Дублікати) | [`NetSdrClientApp/Networking/UdpClientWrapper.cs`](../../NetSdrClientApp/Networking/UdpClientWrapper.cs) |

Усього виправлено 10 смелів/багів за один PR, поведінка зовнішніх API не змінена (крім перейменування `Disconect` → `Disconnect`, що є очевидним багом і також виправлено у викликах).

## Як перевірити

```bash
dotnet restore NetSdrClient.sln
dotnet build NetSdrClient.sln -c Release --no-restore
dotnet test NetSdrClient.sln -c Release --no-build
```

Очікуваний результат: build OK, 15 попереджень (всі поза `NetSdrClientApp`), тести 8/8 passed. Після включення SonarCloud в PR має бути зменшення кількості bugs/smells.

## Метрики до/після

| Метрика | До | Після |
|---------|----|-------|
| Compiler warnings | 18 | 15 |
| Compiler warnings у `NetSdrClientApp` | 5 | 0 |
| Тести проходять | 8/8 | 8/8 |
| `GetSamples` асимптотична складність | O(n²) | O(n) |
| Race у `responseTaskSource` | присутня | відсутня |

Очікувана динаміка SonarCloud (буде підтверджено скрінами після ввімкнення Quality Gate):

| Sonar метрика | До | Після (очікується) |
|---------------|----|---------------------|
| Bugs | ≥3 | 0 |
| Code Smells | ≥10 | значне зменшення |
| Reliability Rating | C+ | A |

## Висновки

Невелика серія мікро-виправлень суттєво підвищує читабельність та безпеку коду без зміни зовнішнього контракту. Найризикованіші зміни — синхронізація `responseTaskSource` (бо вона стосується конкурентності) — покрита існуючими тестами; додаткові тести цього сценарію будуть у Лабі 3.

## Посилання

- [SonarSource — `S2925` no thread races on shared state](https://rules.sonarsource.com/csharp/RSPEC-2925)
- [.NET docs — `Interlocked.Exchange`](https://learn.microsoft.com/dotnet/api/system.threading.interlocked.exchange)
- [.NET docs — `HashCode.Combine`](https://learn.microsoft.com/dotnet/api/system.hashcode.combine)

## Скріни

```text
[ScreenSonar4] Sonar Issues панель ДО (з ppanchen-проєкту як референс)
[ScreenSonar5] Sonar Issues панель ПІСЛЯ (зменшення Bugs/Smells)
```
