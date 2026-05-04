# Лабораторна робота 1. Підключення SonarCloud і CI

**Дисципліна**: Реінжиніринг програмного забезпечення  
**Студент**: Биков Нікіта Вячеславович  
**Група**: ПЗС-1  
**Гілка**: `lab-01-sonarcloud-ci`  
**Pull Request**: створюється з `nik-bykoff:lab-01-sonarcloud-ci` у `lenagrin/ReengineeringCourse:master`

## Мета

Підключити SonarCloud до репозиторію і створити GitHub Actions pipeline, який на кожен push у `master` та pull request виконує сканування коду та публікує звіт у SonarCloud (з декорацією PR і Quality Gate).

## Хід виконання

1. Виконано форк `lenagrin/ReengineeringCourse` у власний акаунт `nik-bykoff/ReengineeringCourse`. Локальні `remote`-и налаштовано так:
   - `origin` -> `https://github.com/nik-bykoff/ReengineeringCourse.git`
   - `upstream` -> `https://github.com/lenagrin/ReengineeringCourse.git`
2. Підготовлено інфраструктуру для збереження локальних секретів (`.env` додано до [`.gitignore`](../../.gitignore)), щоб виключити витік `GITHUB_TOKEN`.
3. У workflow [`.github/workflows/sonarcloud.yml`](../../.github/workflows/sonarcloud.yml) ключі SonarCloud винесено у блок `env`. Поточні значення:
   - `SONAR_PROJECT_KEY = nik-bykoff_ReengineeringCourse`
   - `SONAR_ORGANIZATION = nik-bykoff`
4. У [`README.md`](../../README.md) додано шапку з даними студента, лінком на форк і таблицю звітів усіх восьми лабораторних. Бейджі SonarCloud переведено на `nik-bykoff_ReengineeringCourse`.
5. Закоментований крок `Tests with coverage (OpenCover)` залишено на місці з міткою «буде увімкнено у Лабі 3».

## Зміни у коді та конфігурації

| Файл | Зміна |
|------|-------|
| [`.github/workflows/sonarcloud.yml`](../../.github/workflows/sonarcloud.yml) | Винесено `SONAR_PROJECT_KEY` / `SONAR_ORGANIZATION` в `env`, додано шапку-інструкцію, прибрано зайві коментарі шаблону |
| [`README.md`](../../README.md) | Додано шапку студента, форк/upstream, таблиця звітів, оновлені бейджі |
| [`.gitignore`](../../.gitignore) | Додано правила `.env`, `.env.*`, виняток `.env.example` |
| [`docs/labs/lab-01.md`](lab-01.md) | Цей звіт |

## Як перевірити

1. У SonarCloud створити організацію (якщо ще не створена) та новий проєкт `ReengineeringCourse` з аккаунту `nik-bykoff`. Зафіксувати Project Key та Organization Key.
2. У SonarCloud вимкнути Automatic Analysis: `Project -> Administration -> Analysis Method -> CI-based`.
3. Згенерувати User Token: `My Account -> Security -> Generate Tokens` (тип Project Analysis).
4. Додати токен у Secrets форку: `Repo Settings -> Secrets and variables -> Actions -> New repository secret`, ім'я `SONAR_TOKEN`.
5. Якщо `SONAR_PROJECT_KEY` / `SONAR_ORGANIZATION` у workflow відрізняються від реально створених у SonarCloud — оновити значення у блоці `env`.
6. Створити PR з гілки `lab-01-sonarcloud-ci` у `master`. У вкладці `Checks` PR-а перевірити, що `Sonar Check` та `SonarCloud Code Analysis` пройшли і Quality Gate декоративно прив'язаний до PR.
7. Бейджі у README мають почати показувати реальні значення після першого успішного аналізу.

## Метрики до/після

Цей крок ще не дає метрик якості — він лише вмикає інфраструктуру. Базові показники зафіксовано наприкінці лаби (буде наповнено після першого Sonar-аналізу):

| Метрика | До | Після |
|---------|----|-------|
| Quality Gate | відсутній | очікується `Passed` після фіксів у Лабах 2–8 |
| Coverage on New Code | n/a | n/a (тести з'являться у Лабі 3) |
| Bugs | n/a | n/a |
| Code Smells | n/a | n/a |
| Duplications on New Code | n/a | n/a |

## Висновки

Підключення SonarCloud вимагає чотирьох зовнішніх дій (створення проєкту, токен, secret у GitHub, вимкнення Automatic Analysis), які не автоматизуються через коміт у репозиторій. Усе, що автоматизується (workflow, бейджі, README, гігієна `.env`), оформлено в межах гілки `lab-01-sonarcloud-ci`. Подальші лаби спираються на цей пайплайн.

## Посилання

- Шаблон workflow з README базового завдання
- [SonarCloud для .NET — офіційна документація](https://docs.sonarsource.com/sonarcloud/getting-started/github/)
- [GitHub Actions — `setup-dotnet`](https://github.com/actions/setup-dotnet)

## Скріни

Місця для скрінів (буде вкладено після першого реального запуску):

```text
[ScreenSonar1] Project Information у SonarCloud (Project Key, Organization)
[ScreenSonar2] PR Checks: SonarCloud Code Analysis - Passed
[ScreenSonar3] Бейджі у README з реальними значеннями
```
