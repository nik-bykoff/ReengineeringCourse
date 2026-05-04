# Лабораторна робота 7. Оновлення залежностей

**Дисципліна**: Реінжиніринг програмного забезпечення  
**Студент**: Биков Нікіта Вячеславович  
**Група**: ПЗС-1  
**Гілка**: `lab-07-dependencies`  
**Pull Request**: створюється з `nik-bykoff:lab-07-dependencies` у `lenagrin/ReengineeringCourse:master`

## Мета

Виявити і виправити вразливі залежності, увімкнути GitHub Dependency Graph + Dependabot для автоматичного супроводу версій надалі.

## Хід виконання

### 1. Інвентаризація

Команда `dotnet list NetSdrClient.sln package --outdated --include-transitive` на гілці `lab-06-echoserver-refactor` повідомила про застарілі топ-левел та транзитивні залежності. Зокрема, у [`NetSdrClientApp.csproj`](../../NetSdrClientApp/NetSdrClientApp.csproj):

- `Newtonsoft.Json 13.0.0` — застарілий пакет, який резолвився у `13.0.1` через approximate-match (NU1603). Обʼявлений у `13.0.0` рядок викликав попередження кожного `dotnet restore`.
- `SharpZipLib 1.3.2` — `dotnet restore` видавав три попередження `NU1902`/`NU1903`:
  - [GHSA-2x7h-96h5-rq84](https://github.com/advisories/GHSA-2x7h-96h5-rq84) (moderate)
  - [GHSA-m22m-h4rf-pwq3](https://github.com/advisories/GHSA-m22m-h4rf-pwq3) (high)
  - [GHSA-mm6g-mmq6-53ff](https://github.com/advisories/GHSA-mm6g-mmq6-53ff) (moderate)

### 2. Оновлення версій

У [`NetSdrClientApp.csproj`](../../NetSdrClientApp/NetSdrClientApp.csproj):

```diff
- <PackageReference Include="Newtonsoft.Json" Version="13.0.0" />
- <PackageReference Include="SharpZipLib" Version="1.3.2" />
+ <PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
+ <PackageReference Include="SharpZipLib" Version="1.4.2" />
```

- `Newtonsoft.Json 13.0.3` — мінорне оновлення в межах 13.x; вирішує застереження NU1603 і відповідає поточним рекомендаціям про безпечну версію.
- `SharpZipLib 1.4.2` — мінорне оновлення в межах 1.x; містить виправлення усіх трьох CVE із попереджень.

### 3. Dependabot

Створено [`.github/dependabot.yml`](../../.github/dependabot.yml) з двома еко-системами:

- `nuget` (`directory: /`) — щотижневі PR-и (`monday 08:00 Europe/Kyiv`); група `test-stack` (NUnit*/Test.Sdk/coverlet/Moq) обʼєднує дрібні апдейти у один PR замість шести окремих.
- `github-actions` (`directory: /`) — щотижневі PR-и для оновлення SHA/версій у workflow.

Конфіг також задає `labels: dependencies, nuget|github-actions`, `commit-message.prefix: deps(...)`, `open-pull-requests-limit: 5`.

### 4. Що залишається зробити вручну (документую)

1. У `Settings -> Code security and analysis` форку увімкнути:
   - Dependency graph
   - Dependabot alerts
   - Dependabot security updates
2. Після пушу гілки Dependabot побачить `dependabot.yml` і за хвилини почне scan, видасть алерт у `Security -> Dependabot` (історичний — для версій до фіксу).
3. Через тиждень почнуть приходити плановані PR-и (test-stack, окремі топ-левел NuGet-и, github-actions).

## Зміни у коді та конфігурації

| Файл | Зміна |
|------|-------|
| [`NetSdrClientApp/NetSdrClientApp.csproj`](../../NetSdrClientApp/NetSdrClientApp.csproj) | `Newtonsoft.Json` 13.0.0 -> 13.0.3, `SharpZipLib` 1.3.2 -> 1.4.2 |
| [`.github/dependabot.yml`](../../.github/dependabot.yml) | новий — `nuget` (з групою `test-stack`) + `github-actions` |
| [`docs/labs/lab-07.md`](lab-07.md) | цей звіт |

## Як перевірити

```bash
dotnet restore NetSdrClient.sln
dotnet build NetSdrClient.sln -c Release --no-restore
dotnet test NetSdrClient.sln -c Release --no-build
```

Очікуваний результат:

```text
0 Warning(s)
0 Error(s)

Passed!  - Failed: 0, Passed: 18, ... NetSdrClientAppTests.dll
Passed!  - Failed: 0, Passed:  4, ... NetSdrClient.ArchTests.dll
Passed!  - Failed: 0, Passed: 10, ... EchoTcpServer.Tests.dll
```

## Метрики до/після

| Метрика | До | Після |
|---------|----|-------|
| Compiler/NuGet warnings (solution-level) | 15 (NU1603 / NU1902 / NU1903) | 0 |
| Vulnerable production deps (`NetSdrClientApp`) | 1 (`SharpZipLib`) | 0 |
| Approximate-match resolutions (`NU1603`) | `Newtonsoft.Json 13.0.0 -> 13.0.1` | відсутні |
| Тести проходять | 32/32 | 32/32 |
| Dependabot активний | ні | так |

## Ризики мажорних апдейтів (для майбутніх PR-ів від Dependabot)

- `NUnit 3.x -> 4.x` — реальний breaking change (нові attributes, видалені класи). Тримаємо у групі `test-stack`, апдейтимо одночасно.
- `Microsoft.NET.Test.Sdk 17.x -> 18.x` — потребує сумісних NUnit3TestAdapter/NUnit3 версій; також у групі `test-stack`.
- `Moq` — деякі мажори ламають API; локально тести треба перезапустити.

Усі продакшн-залежності (`Newtonsoft.Json`, `SharpZipLib`) оновлюються лише у межах мажора і несуть мінімальний ризик.

## Висновки

Виправили реальну вразливу залежність (`SharpZipLib`), прибрали approximate-match попередження для `Newtonsoft.Json`, увімкнули Dependabot як інструмент постійного контролю. Усі 32 тести залишились зеленими, нуль попереджень при білді — це передумова до зеленого Quality Gate у Лабі 8.

## Посилання

- [GitHub Docs — Dependabot configuration options](https://docs.github.com/en/code-security/dependabot/dependabot-version-updates/configuration-options-for-the-dependabot.yml-file)
- [SharpZipLib GHSA-m22m-h4rf-pwq3](https://github.com/advisories/GHSA-m22m-h4rf-pwq3)
- [Newtonsoft.Json release notes](https://github.com/JamesNK/Newtonsoft.Json/releases)

## Скріни

```text
[ScreenSecurity1] Settings -> Code security with all toggles ON
[ScreenSecurity2] Dependabot alerts -> resolved після оновлення
[ScreenAction4] dependabot[bot] PR з оновленням test-stack групи
```
