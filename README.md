# Course Project: Reference book handler

A C# + WPF application for managing product data and sales records, developed as a university course project.

## Available Languages
- [English](README.md)
- [Русский](README.ru.md)

## Features

### Product and Sales Management
- Add, edit, and delete products and sales records

### Data Operations
- **Search**:
  Uses AVL-tree for fast searching by both date and product article number
- **File operations**:
  - Save data to text files
  - Load data from files
- View all products and sales in tables

## Implementation
- **C#** programming language with **WPF** for user interface
- **Hash table** for storing products
- **AVL-tree** for searching and storing sales data

## File Formats

### Goods File (products.txt)
[Article Number];[Product Name];[Price]

Example:
```
  EL-10001;Smartphone X;499,99
  CL-20012;Jeans Classic;59,50
  FUR-30023;Office Chair;129,00
  OTH-40034;LED Lamp;19,90
```

### Sales File (sales.txt)
[Article Number];[Quantity];[Seller];[Date]

Example:
```
EL-10001;5;Alexander;01.01.2020
CL-20012;3;Maria;17.05.2024
EL-10045;9;Pavel;17.05.2024
FUR-30067;2;Anna;17.03.2021
```

### Format Rules:
1. One entry per line
2. Fields separated by semicolons (;)
3. Date format: DD.MM.YYYY

## How to Run

1. Clone repository:
  ```
  bash
  git clone https://github.com/your-username/product-management.git
  ```
2. Open solution in Visual Studio
3. Build and run the project

## Screenshots
(Add application screenshots here)

## License
MIT
