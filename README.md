# 🐶 DoggoSchoolApp – Application de gestion d’école canine

> Plateforme de gestion pour écoles canines – ASP.NET Core MVC + Entity Framework Core  
> 📆 Gestion de cours, sessions, inscriptions canines | 👥 Rôles multiples (admin / prof / client)

---

## ✨ Fonctionnalités principales

### 👥 Gestion des utilisateurs (Identity)
- Authentification sécurisée (ASP.NET Core Identity)
- Liaison `IdentityUser ↔ Person` pour stocker les données complètes des utilisateurs
- Rôles stockés en base (Admin, Professeur, Client)

### 🐕 Chiens
- Chaque utilisateur client peut enregistrer plusieurs chiens
- Données : âge, taille, poids
- Vérification du propriétaire dans toutes les actions

### 📘 Cours
- Créés par les professeurs
- Possèdent des **critères d'admission** :
  - Tranches d'âge (mois)
  - Taille et poids min/max
- Validation obligatoire par un admin pour être visibles

### 📝 Inscriptions (Register)
- Inscription d’un chien à un cours compatible (si critères OK)
- **Deux étapes** :  
  1. Inscription à un cours (en attente admin)  
  2. Puis inscription à une session

### 📆 Sessions
- Créées par les professeurs pour un cours
- Contiennent : date, heure, nombre max de chiens
- Vérifications métier :
  - Un chien doit être inscrit au cours
  - Pas déjà inscrit sur une session qui se chevauche
  - Place disponible

### 📅 Agenda glissant
- Vue dynamique semaine par semaine
- Deux modes :
  - Sessions disponibles tous cours confondus
  - Sessions d’un cours spécifique
- Navigation entre les semaines

### 🔐 Sécurité & règles métier
- Vérification du rôle et des droits sur chaque action
- Validation des critères avant chaque inscription
- Adaptation des affichages selon le rôle connecté
- Empêche les conflits d’horaires pour les chiens **et** les professeurs

---

## ⚙️ Architecture technique

- **Backend** : ASP.NET Core MVC
- **ORM** : Entity Framework Core + SQL Server
- **Services métier** pour logique métier :
  - `RegisterService`, `ParticipateService`, `CourseService`, etc.
- **Injection de dépendances** via interfaces (`IRegisterService`…)
- **Tests unitaires** : MSTest ou xUnit
- **Front** : Razor Pages + TailwindCSS (partiel)

---

## ✅ Fonctionnalités finalisées

| Fonctionnalité | Statut |
|--|--|
| Authentification & gestion des rôles | ✅ |
| CRUD Chiens, Cours, Sessions | ✅ |
| Inscriptions aux cours avec vérif. critères | ✅ |
| Agenda sessions glissant avec filtres | ✅ |
| Sécurité : rôles, propriétaire, validations | ✅ |
| Tests unitaires pour services critiques | ✅ |


---

## 🧪 Tests unitaires

Les services `RegisterService` et `ParticipateService` ont été testés avec des bases InMemory pour vérifier les règles métier :
- Inscription refusée si critères non remplis
- Empêche les conflits horaires
- Tests de succès et d’échec avec MSTest

---

## 📁 Arborescence simplifiée

```
DoggoSchoolApp/
├── Controllers/
├── Models/
├── ViewModels/
├── Views/
├── Services/
│   ├── IRegisterService.cs
│   ├── RegisterService.cs
│   └── ...
├── Data/
│   └── ApplicationDbContext.cs
├── Tests/
│   └── RegisterServiceTests.cs
├── wwwroot/
│   └── TailwindCSS (custom)
├── Program.cs
├── appsettings.json
└── README.md
```

---

## 🧾 À venir
- Page “Mon profil” modifiable
- Interface publique (landing page)
- Plus de tests unitaires
- Refonte graphique avec Tailwind

---

## 📜 License

MIT
