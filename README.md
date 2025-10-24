# 🏨 NovaStay Hotel Reservation System

NovaStay Hotel is a modern **WPF (Windows Presentation Foundation)** application designed to streamline hotel operations.  
It allows receptionists to manage rooms, guests, and reservations efficiently through a clean, responsive interface powered by **Telerik UI** and backed by **Entity Framework Core** and **SQL Server**.

---

## 🚀 Features

### 🧑‍💼 Guest Management
- Add, edit, and delete guest records.
- Search guests by name or phone number.
- Store essential information such as nationality, passport number, date of birth, and contact details.

### 🏠 Room Management
- Add and update room details (number, type, floor, price, balcony, and status).
- Filter rooms by type, status, floor, or price range.
- Track room maintenance and availability.

### 📅 Reservation Management
- Create, update, or cancel reservations with conflict validation.
- Automatically calculate total cost based on room price, nights stayed, and discounts.
- Validate booking periods and enforce business rules (e.g., date overlaps, room availability).
- Track reservation status: **Created**, **Checked In**, **Checked Out**, **Canceled**.

---

## 🧠 Technologies Used

| Layer | Technology |
|-------|-------------|
| **Frontend (UI)** | WPF (C#), Telerik UI for WPF |
| **Backend (Logic & Services)** | C# (.NET 8), Entity Framework Core |
| **Database** | Microsoft SQL Server |
| **Architecture** | Clean Layered Architecture (Models, Services, Data Context) |
| **Testing** | Unit tests for services and database validation |

---

---

## 🧾 Database & Entity Framework

- Database: **SQL Server**
- ORM: **Entity Framework Core**
- Automatically creates and migrates the database.
- Each entity includes timestamps (`CreatedAt`, `UpdatedAt`) and validation logic.

---

## 🧰 Key Service Classes

### `GuestService.cs`
Handles CRUD operations and validation for hotel guests.

### `RoomService.cs`
Manages room information, filtering, and conflict checks.

### `ReservationService.cs`
Implements reservation logic with:
- Availability checks  
- Date overlap detection  
- Amount calculation (Base & Final)  
- Status transitions and business rule enforcement

---

## 🧪 Testing

Each service (Guest, Room, Reservation) was tested for:
- Database integration (EF Core)
- Business rule validation
- Error handling and exception flow

---

## 🎨 UI & UX

- Built using **Telerik WPF controls** for a smooth and modern design.
- Three main pages:
  1. **Room Management Page**
  2. **Guest Management Page**
  3. **Reservation Management Page**
  4. **Home Page **

---

## 🧍 Author

**👤 Anas Mardoud**  
💼 Backend & Desktop Application Developer  
📧 [anas.mardoud.cs@gmail.com]  
🌐 [https://www.linkedin.com/in/anas-mardoud-47996222a/]

---

> “A reliable, efficient, and elegant WPF application for hotel management built with .NET, EF Core, and SQL Server.”


