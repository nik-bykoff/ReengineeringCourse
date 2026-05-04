# Лабораторна робота 4. Дублікати через SonarCloud

**Дисципліна**: Реінжиніринг програмного забезпечення  
**Студент**: Биков Нікіта Вячеславович  
**Група**: ПЗС-1  
**Гілка**: `lab-04-duplications`  
**Pull Request**: створюється з `nik-bykoff:lab-04-duplications` у `lenagrin/ReengineeringCourse:master`

## Мета

Зменшити дублікати коду до рівня, нижчого за поріг SonarCloud (Duplications on New Code не більше 3%) шляхом виокремлення спільних блоків у окремі helper-методи/класи.

## Виявлені дублікати (до)

1. У [`TcpClientWrapper.cs`](../../NetSdrClientApp/Networking/TcpClientWrapper.cs) перевантажені `SendMessageAsync(byte[])` та `SendMessageAsync(string)` — обидві повторювали 6 рядків (перевірка стану, лог, `WriteAsync`). Це класичний `S4144` / Sonar duplication block.
2. У [`UdpClientWrapper.cs`](../../NetSdrClientApp/Networking/UdpClientWrapper.cs) методи `StopListening` та `Exit` мали повністю однакові тіла (cancel + close + log + catch).
3. Hex-форматування `data.Select(b => Convert.ToString(b, toBase: 16))` було продубльоване у трьох місцях (TCP send, TCP receive log, UDP samples log) у `TcpClientWrapper.cs` та `NetSdrClient.cs`.

## Виконані зміни

### 1. SendMessageAsync overloads -> SendCoreAsync

Винесено приватний метод [`TcpClientWrapper.SendCoreAsync`](../../NetSdrClientApp/Networking/TcpClientWrapper.cs):

```csharp
public Task SendMessageAsync(byte[] data) => SendCoreAsync(data);
public Task SendMessageAsync(string str) => SendCoreAsync(Encoding.UTF8.GetBytes(str));

private async Task SendCoreAsync(byte[] data)
{
    if (!Connected || _stream is null || !_stream.CanWrite)
    {
        throw new InvalidOperationException("Not connected to a server.");
    }
    Console.WriteLine("Message sent: " + HexFormatter.ToSpaceSeparatedHex(data));
    await _stream.WriteAsync(data, 0, data.Length);
}
```

### 2. StopListening / Exit -> StopCore

Обидва публічних методи делегують у приватний [`UdpClientWrapper.StopCore`](../../NetSdrClientApp/Networking/UdpClientWrapper.cs):

```csharp
public void StopListening() => StopCore();
public void Exit() => StopCore();

private void StopCore() { /* спільне тіло */ }
```

Контракт `IUdpClient` не змінився, тому існуючі тести не модифіковано.

### 3. Hex-формат -> HexFormatter

Створено [`NetSdrClientApp/Networking/HexFormatter.cs`](../../NetSdrClientApp/Networking/HexFormatter.cs) як `internal static`-helper із одним методом `ToSpaceSeparatedHex(byte[])`. Ним користуються:
- `TcpClientWrapper.SendCoreAsync` — лог відправлення;
- `NetSdrClient._udpClient_MessageReceived` — лог samples;
- `NetSdrClient._tcpClient_MessageReceived` — лог response.

## Як перевірити

```bash
dotnet build NetSdrClient.sln -c Release --no-restore
dotnet test NetSdrClient.sln -c Release --no-build
```

Очікуваний результат: build OK, 18/18 тестів passed (без змін у тестовій логіці).

У SonarCloud: вкладка `Measures -> Duplications` має показати 0% `Duplications on New Code` (PR-сторінка) і Quality Gate на цьому пункті — зелений.

## Метрики до/після

| Метрика | До | Після |
|---------|----|-------|
| Дублікат-блок `SendMessageAsync` | 1 пара (~12 LOC) | 0 |
| Дублікат-блок `StopListening` / `Exit` | 1 пара (~10 LOC) | 0 |
| Дублікати hex-логування | 3 копії | 1 helper |
| Тести проходять | 18/18 | 18/18 |

Очікувано у Sonar:

| Sonar метрика | До | Після (очікується) |
|---------------|----|---------------------|
| `Duplications on New Code` | >3% | 0% |
| `Duplicated Lines` (NetSdrClientApp) | ~25 | 0 |

## Висновки

Дрібні дублікати в `Networking`-шарі усунено через стандартну техніку Extract Method + Extract Class. Контракт публічних API не змінився, поведінка ідентична, тести залишаються 18/18 зеленими. Тепер у `NetSdrClientApp` нема жодного очевидного дубль-блоку.

## Посилання

- [SonarSource — `S4144` methods should not have identical implementations](https://rules.sonarsource.com/csharp/RSPEC-4144)
- [SonarCloud — Duplications](https://docs.sonarsource.com/sonarcloud/digging-deeper/duplications/)

## Скріни

```text
[ScreenSonar8] Sonar Duplications до — більший за 3% на New Code
[ScreenSonar9] Sonar Duplications після — 0% на цьому PR
```
