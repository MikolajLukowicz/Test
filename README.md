# allegro 2.0 - System Aukcyjny

Projekt desktopowy wykonany w technologii WPF, .NET 8.0 oraz Entity Framework Core.

## Konfiguracja Połączenia z Bazą Danych
Aby aplikacja działała poprawnie, musisz upewnić się, że connection string pasuje do Twojego środowiska. Konfiguracja znajduje się w pliku:
`SystemAukcyjny.Wpf/App.xaml.cs` (linijka 18).

Domyślne ustawienie:
```csharp
string connectionString = "Server=DESKTOP-S98QKHV;Database=SystemAukcyjny;Trusted_Connection=True;TrustServerCertificate=True;";
```

## Funkcje Aplikacji
- **Logowanie i Rejestracja:** Bezpieczny dostęp do konta.
- **Przeglądanie Aukcji:** Widok wszystkich aktywnych ofert z filtrowaniem po kategorii.
- **Licytowanie:** Możliwość przebijania ofert (obsługa kwot z przecinkiem i kropką).
- **Panel Użytkownika:**
  - **Moje Wystawione:** Zarządzanie własnymi ofertami (usuwanie, kończenie).
  - **Licytowane:** Lista aukcji, w których bierzesz udział.
  - **Wygrane:** Lista aukcji zakończonych Twoim zwycięstwem wraz z kwotami.

## Technologie
- **C# / WPF**
- **MVVM** (CommunityToolkit.Mvvm)
- **EF Core** (SQL Server)
- **Dependency Injection** (Microsoft.Extensions.DependencyInjection)
