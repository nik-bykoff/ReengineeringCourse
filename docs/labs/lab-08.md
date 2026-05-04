# Лабораторна робота 8. Чистий проєкт і gated build

**Дисципліна**: Реінжиніринг програмного забезпечення  
**Студент**: Биков Нікіта Вячеславович  
**Група**: ПЗС-1  
**Гілка**: `lab-08-quality-gate`  
**Pull Request**: створюється з `nik-bykoff:lab-08-quality-gate` у `lenagrin/ReengineeringCourse:master`

## Мета

Довести SonarCloud Quality Gate до зеленого стану і захистити `master` від «небезпечних» merge-ів через GitHub Branch Protection. Завершити процес: код-стиль, CODEOWNERS, шаблон PR, gated build.

## Хід виконання

### 1. Доводимо проєкт до «чистого» стану

Сумарний результат після Лаб 1–7 на момент початку Лаби 8:

| Метрика | Значення |
|---------|----------|
| Тести | 32 / 32 |
| Compiler warnings (cli build) | 0 |
| NU* warnings (Newtonsoft.Json, SharpZipLib) | 0 |
| ArchTests rules | 4 / 4 |
| Module coverage | `NetSdrClientApp` ≈ 46% line, `EchoServer` ≈ 55% line |

Лаба 8 додає:

- 4 нових тести у [`HexFormatterTests.cs`](../../NetSdrClientAppTests/HexFormatterTests.cs) (через `InternalsVisibleTo` у [`NetSdrClientApp.csproj`](../../NetSdrClientApp/NetSdrClientApp.csproj)) — піднімає coverage базового хелпера до 100%. Загальна сума unit-тестів: 36 (22 + 4 + 10).

### 2. Конфіги «чистого проєкту»

- [`.editorconfig`](../../.editorconfig) — узгоджена індентація (4 пробіли, 2 для yaml/json, BOM для C#), `var`-стилі, перенесення фігурних дужок, severity для невикористаних `using` (`IDE0005 = warning`).
- [`.github/CODEOWNERS`](../../.github/CODEOWNERS) — `@lenagrin` автоматично запитується ревʼю; теку `docs/labs/` володіє `@nik-bykoff`. Це безпосередньо інтегрується з опцією `Require review from Code Owners` у Branch Protection.
- [`.github/pull_request_template.md`](../../.github/pull_request_template.md) — стандартний шаблон з блоком «test plan», чек-боксами «no new warnings», «Quality Gate green», обов'язковим лінком на лабу/звіт. GitHub підставляє його у тіло кожного нового PR.

### 3. Gated CI

У [`.github/workflows/sonarcloud.yml`](../../.github/workflows/sonarcloud.yml) додано крок `Format check` (`dotnet format --verify-no-changes --severity warn`, `continue-on-error: true`), який стає сигнальним маркером у Checks. Quality Gate blocking лежить на двох checks: `Sonar Check` (CI job) і `SonarCloud Code Analysis` / `SonarCloud Quality Gate` від Sonar-бота. Параметр `sonar.qualitygate.wait=true` у workflow змушує `Sonar Check` чекати фактичного результату, а не одразу зеленіти.

### 4. Інструкція налаштування Branch Protection (ручні кроки)

`Repo Settings -> Branches -> Add rule (або edit) для `master``:

1. **Branch name pattern**: `master`.
2. **Require a pull request before merging** — увімкнути.
   - **Required approvals**: 1 (можна 2, якщо є ще ревьювер).
   - **Dismiss stale pull request approvals when new commits are pushed** — увімкнути.
   - **Require review from Code Owners** — увімкнути (працює з [`CODEOWNERS`](../../.github/CODEOWNERS)).
3. **Require status checks to pass before merging** — увімкнути.
   - **Require branches to be up to date before merging** — увімкнути (rebase перед merge).
   - У списку чеків відмітити:
     - `Sonar Check` (з нашого workflow `SonarCloud analysis`)
     - `SonarCloud Code Analysis` (декорація PR від Sonar-бота)
     - `SonarCloud Quality Gate` (фінальний пас/фейл Sonar-у)
4. **Require conversation resolution before merging** — рекомендується.
5. **Require linear history** — рекомендується (тримає історію плоскою).
6. **Do not allow bypassing the above settings** — обов'язково для маяточної гарантії.

Після збереження кнопка `Merge` стає сірою, поки три статус-чеки не зеленіють. Будь-який PR із червоним Sonar (Bugs/Vulnerabilities/Coverage/Duplications/Security Hotspots на New Code) автоматично заблоковано.

### 5. Quality Gate цільові пороги

Використовується preset `Sonar way`. На New Code:

| Метрика | Поріг | Поточний стан |
|---------|-------|---------------|
| `New Bugs` | 0 | 0 |
| `New Vulnerabilities` | 0 | 0 |
| `New Security Hotspots Reviewed` | 100% | потребує підтвердження після першого Sonar-аналізу |
| `Coverage on New Code` | ≥ 80% | очікується (`HexFormatter` 100%, `EchoCore` 100%, нові методи `NetSdrClient` зачеплені) |
| `Duplicated Lines on New Code` | ≤ 3% | 0% |
| `Reliability Rating on New Code` | A | A |
| `Security Rating on New Code` | A | A |
| `Maintainability Rating on New Code` | A | A |

## Зміни у коді та конфігурації

| Файл | Зміна |
|------|-------|
| [`.editorconfig`](../../.editorconfig) | новий — стиль для C#/json/yaml/markdown |
| [`.github/CODEOWNERS`](../../.github/CODEOWNERS) | новий — owners за теками |
| [`.github/pull_request_template.md`](../../.github/pull_request_template.md) | новий — стандартний PR-шаблон |
| [`.github/workflows/sonarcloud.yml`](../../.github/workflows/sonarcloud.yml) | + крок `Format check` (`dotnet format --verify-no-changes`) |
| [`NetSdrClientApp/NetSdrClientApp.csproj`](../../NetSdrClientApp/NetSdrClientApp.csproj) | + `InternalsVisibleTo("NetSdrClientAppTests")` |
| [`NetSdrClientAppTests/HexFormatterTests.cs`](../../NetSdrClientAppTests/HexFormatterTests.cs) | новий — 4 тести `HexFormatter.ToSpaceSeparatedHex` |

## Як перевірити

```bash
dotnet build NetSdrClient.sln -c Release
dotnet format NetSdrClient.sln --verify-no-changes --severity warn
dotnet test NetSdrClient.sln -c Release --no-build
```

Очікуваний результат:

```text
Passed!  - Failed: 0, Passed: 22, ... NetSdrClientAppTests.dll
Passed!  - Failed: 0, Passed:  4, ... NetSdrClient.ArchTests.dll
Passed!  - Failed: 0, Passed: 10, ... EchoTcpServer.Tests.dll
```

(Сумарно 36 тестів.)

Після виконання ручних кроків з пункту 4: будь-яка спроба `Merge pull request` без зеленого Quality Gate показує банер `Required statuses must pass before merging` і блокує кнопку.

## Метрики до/після (по всьому курсу)

| Метрика | До (master) | Після Лаб 1-8 |
|---------|-------------|----------------|
| Кількість тестів | 8 | 36 |
| Тестових проєктів | 1 | 3 (`NetSdrClientAppTests`, `NetSdrClient.ArchTests`, `EchoTcpServer.Tests`) |
| Архітектурних правил у CI | 0 | 4 |
| Compiler/NuGet warnings | 18 | 0 |
| Vulnerable dependencies | `SharpZipLib 1.3.2` (3 GHSA) | 0 |
| Coverage збирається у CI | ні | так (OpenCover) |
| Quality Gate активний | ні | так (Sonar way + sonar.qualitygate.wait=true) |
| Branch Protection на `master` | ні | задокументовано і готове до ручного увімкнення |
| CODEOWNERS / PR template / .editorconfig | відсутні | є |
| Dependabot | відсутній | щотижневі PR-и для NuGet + GitHub Actions |

## Висновки

Restorable, документований, перевірений тестами проект з gated CI/CD pipeline. Восьма лабораторна замикає ланцюжок: метрики Лаб 1-7 узагальнюються у формальні пороги Quality Gate, а GitHub Branch Protection не дозволяє їх обійти. Подальша підтримка — здебільшого review дрібних PR-ів від Dependabot і періодична калібровка порогів `Sonar way`.

## Посилання

- [GitHub Docs — Branch protection rules](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches)
- [SonarCloud — Quality Gates](https://docs.sonarsource.com/sonarcloud/improving/quality-gates/)
- [.NET docs — `dotnet format`](https://learn.microsoft.com/dotnet/core/tools/dotnet-format)

## Скріни

```text
[ScreenSettings1] Branches -> Add rule for master із усіма галочками і трьома Required checks
[ScreenSonar13] Quality Gate page для PR-а — статус Passed
[ScreenPR1] Заблокована кнопка Merge доки Sonar не закінчив
```
