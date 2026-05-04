# Pull Request

## Summary

<!--
What does this PR change and why? Reference the lab number, the file(s)
touched and the architectural intent. Two or three sentences are enough.
-->

## Linked lab / report

- Lab: <!-- e.g. Lab 4 - Duplications -->
- Report: <!-- e.g. docs/labs/lab-04.md -->

## Changes

- [ ] Production code (`NetSdrClientApp/` or `EchoTcpServer/`)
- [ ] Tests (`*Tests/`)
- [ ] CI / Sonar / Dependabot (`.github/`)
- [ ] Documentation (`docs/`, `README.md`)

## Test plan

```
# How to verify locally:
dotnet build NetSdrClient.sln -c Release
dotnet test NetSdrClient.sln -c Release --no-build
```

- [ ] All existing tests still pass
- [ ] New tests added or existing tests updated where appropriate
- [ ] No new compiler / NuGet warnings introduced
- [ ] SonarCloud Quality Gate is green (`Sonar Check`, `SonarCloud Code Analysis`, `SonarCloud Quality Gate`)

## Risks and rollback

<!-- Anything reviewers should pay extra attention to (concurrency, IO, public API).
     How would we revert this change? -->

## Discipline / Author

- Subject: Реінжиніринг програмного забезпечення
- Student: Биков Нікіта Вячеславович, group ПЗС-1
