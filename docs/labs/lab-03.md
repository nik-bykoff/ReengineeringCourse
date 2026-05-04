# Лабораторна робота 3. Тести та покриття

**Дисципліна**: Реінжиніринг програмного забезпечення  
**Студент**: Биков Нікіта Вячеславович  
**Група**: ПЗС-1  
**Гілка**: `lab-03-tests-coverage`  
**Pull Request**: створюється з `nik-bykoff:lab-03-tests-coverage` у `lenagrin/ReengineeringCourse:master`

## Мета

Підняти покриття коду юніт-тестами у `NetSdrClientApp`, увімкнути генерацію OpenCover-звіту через `coverlet.msbuild` і пробросити цей звіт у SonarCloud через CI.

## Хід виконання

1. У [`NetSdrClientAppTests.csproj`](../../NetSdrClientAppTests/NetSdrClientAppTests.csproj) додано пакет `coverlet.msbuild` 6.0.0 (з `PrivateAssets=all`, щоб не перетворювати тестовий проєкт на бібліотеку розповсюдження).
2. У [`NetSdrClientTests.cs`](../../NetSdrClientAppTests/NetSdrClientTests.cs) додано 4 нові тести:
   - `ChangeFrequencyAsync_SendsExactlyOneTcpMessage`
   - `StopIQ_WhenNotConnected_DoesNotSend`
   - `StartIQ_AfterConnect_StartsUdpListenerOnce`
   - `StartThenStopIQ_TogglesIQStartedFlag`
3. У [`NetSdrMessageHelperTests.cs`](../../NetSdrClientAppTests/NetSdrMessageHelperTests.cs) додано 6 нових тестів навколо `TranslateMessage` / `GetSamples`:
   - `TranslateMessage_ControlItemRoundtrip_PreservesTypeCodeAndBody`
   - `TranslateMessage_DataItemRoundtrip_ExtractsSequenceNumber`
   - `GetSamples_With16BitWidth_YieldsExpectedCount`
   - `GetSamples_With8BitWidth_TruncatesIncompleteTail`
   - `GetSamples_OnEmptyBody_ReturnsEmptySequenceWithoutThrowing`
   - `GetSamples_With40BitWidth_ThrowsArgumentOutOfRange`
4. Тест `TranslateMessage_ControlItemRoundtrip_PreservesTypeCodeAndBody` викрив прихований баг у `NetSdrMessageHelper.TranslateMessage`: `Enum.IsDefined(typeof(ControlItemCodes), ushortValue)` кидав `ArgumentException`, бо underlying-тип `ControlItemCodes` — `int`. Виправлено через explicit-cast у `(int)value`.
5. У [`.github/workflows/sonarcloud.yml`](../../.github/workflows/sonarcloud.yml) розкоментовано / переписано крок `Tests with coverage (OpenCover)`:
   - формат `opencover`,
   - параметр виключення `Program` (точка входу — без сенсу покривати),
   - вихідний файл `TestResults/coverage.xml`, який підбирається `sonar.cs.opencover.reportsPaths=**/coverage.xml`.

## Зміни у коді та конфігурації

| Файл | Зміна |
|------|-------|
| [`NetSdrClientAppTests/NetSdrClientAppTests.csproj`](../../NetSdrClientAppTests/NetSdrClientAppTests.csproj) | + `coverlet.msbuild` 6.0.0 |
| [`NetSdrClientAppTests/NetSdrClientTests.cs`](../../NetSdrClientAppTests/NetSdrClientTests.cs) | + 4 тести покриття поведінки `NetSdrClient` |
| [`NetSdrClientAppTests/NetSdrMessageHelperTests.cs`](../../NetSdrClientAppTests/NetSdrMessageHelperTests.cs) | + 6 тестів roundtrip і граничних випадків |
| [`NetSdrClientApp/Messages/NetSdrMessageHelper.cs`](../../NetSdrClientApp/Messages/NetSdrMessageHelper.cs) | Каст `(int)value` у `Enum.IsDefined`, щоб уникнути `ArgumentException` |
| [`.github/workflows/sonarcloud.yml`](../../.github/workflows/sonarcloud.yml) | Активний крок `Tests with coverage` з OpenCover |

## Як перевірити

Локальний запуск тестів і покриття:

```bash
cd NetSdrClientAppTests
dotnet test -c Release \
  /p:CollectCoverage=true \
  /p:CoverletOutput=TestResults/coverage.xml \
  /p:CoverletOutputFormat=opencover \
  /p:Exclude="[NetSdrClientApp]NetSdrClientApp.Program"
```

Очікуваний вивід (фактично виміряно):

```
Passed!  - Failed: 0, Passed: 18, Skipped: 0, Total: 18, Duration: ~90 ms

+-----------------+--------+--------+--------+
| Module          | Line   | Branch | Method |
+-----------------+--------+--------+--------+
| NetSdrClientApp | 45.86% | 26.92% | 48.48% |
+-----------------+--------+--------+--------+
```

У CI Sonar тепер бачить файл `**/coverage.xml` і відображає Coverage у вкладці `Measures`.

## Метрики до/після

| Метрика | До | Після |
|---------|----|-------|
| Кількість unit-тестів | 8 | 18 |
| Тести проходять | 8/8 | 18/18 |
| Line coverage `NetSdrClientApp` | n/a (не вимірювалось) | 45.86% |
| Branch coverage | n/a | 26.92% |
| Method coverage | n/a | 48.48% |
| Прихований баг у `TranslateMessage` (control-item) | присутній | виправлено |

Низький рівень покриття зумовлений `TcpClientWrapper`/`UdpClientWrapper`, які інтегрують з ОС-сокетами і потребують рефакторингу для тестування. Це буде зроблено у Лабі 6 (а аналогічний підхід для `EchoServer` уже там запланований). До тих пір `NetSdrClient`/`NetSdrMessageHelper` (бізнес-логіка) покриті значно краще.

## Висновки

Додавання `coverlet.msbuild` та 10 нових тестів дало вимірюване покриття у Sonar і одразу викрило прихований баг в існуючому коді `TranslateMessage`. Це показовий приклад того, як unit-тести працюють як «детектор регресій» у спадковому коді.

## Посилання

- [Coverlet — налаштування MSBuild](https://github.com/coverlet-coverage/coverlet/blob/master/Documentation/MSBuildIntegration.md)
- [SonarCloud — `sonar.cs.opencover.reportsPaths`](https://docs.sonarsource.com/sonarcloud/enriching/test-coverage/dotnet-test-coverage/)

## Скріни

```text
[ScreenSonar6] Sonar Measures — Coverage on New Code
[ScreenSonar7] Sonar Activity — графік росту Coverage
[ScreenAction1] CI лог кроку Tests with coverage у GitHub Actions
```
