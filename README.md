---
title: Tayana Yachts
type: Backend Practice Project
version: 1.0.0
description: ASP.NET MVC 5 yacht website and admin CMS built as a backend practice project.
developer: Joanne H 
---

# 火箭隊培訓營 22梯後端 遊艇專案

**免責聲明**

本專案為參加火箭隊培訓營後端組期間所完成的練習作品，僅作為學習與作品集展示用途。專案內容著重於 ASP.NET MVC 5 後端開發、SQL Server資料庫設計、後台內容流程、登入驗證及檔案上傳。本專案並非 Tayana Yachts 官方網站，亦未與該品牌有任何商業合作或授權關係。

## 簡介
TayanaYachts(遊艇專案)是一個以 ASP.NET Framework 4.8 MVC 5 建立的品牌官網與後台內容管理系統。
- 前台: 提供遊艇型號、最新消息、經銷商、公司資訊與聯絡表單
- 後台: 提供遊艇相關(型號、消息、經銷商)內容管理、會員管理與聯絡紀錄管理。

## 專案亮點
- 使用 ASP.NET MVC 5 建立前台網站與後台內容管理平台
- 以 Entity Framework 6 Code First 設計關聯資料模型
- 實作 Forms Authentication 後台登入與權限保護
- 支援內容發布、檔案上傳、圖片管理與實體檔案與資料庫更新同步清理
- 使用 reCAPTCHA 驗證、聯絡表單保存與 SMTP Email 通知

## 技術

- ASP.NET Framework 4.8, ASP.NET MVC 5, C#
- Entity Framework 6 Code First, SQL Server
- Razor Views, Bootstrap 5, jQuery
- 後台模板: AdminLTE
- 後台內容編輯工具: Summernote WYSIWYG編輯器
- 聯絡郵件寄發套件: MailKit, MimeKit
- 驗證: Forms Authentication (後台), Google reCAPTCHA (聯絡表單填寫), Anti-forgery token
- 分頁: PagedList + PagedList.Mvc

## 功能

### 前台

- 首頁: 遊艇形象照輪播、最新消息(3則)
- 遊艇列表與詳細頁
  - Overview, Dimension及檔案下載
  - Layout and Deck Plan
  - Specification
  - Interior圖片(輪播)
- 最新消息: 列表與詳細頁，支援置頂與發布日期控制
- 經銷商: 依國家與區域分類提供列表
- 公司頁: 公司介紹與證書
- 聯絡表單: 包含驗證、reCAPTCHA、後端資料庫保存與 Email 通知

### 後台

- `Areas/Admin` 後台內容管理平台，使用 Forms Authentication驗證保護
- 管理遊艇、新聞、經銷商、國家、區域、會員與聯絡紀錄
- 遊艇與新聞支援搜尋、分頁與發布狀態控制
- 新聞支援置頂
- 聯絡紀錄支援完成狀態與Soft Delete
- 會員密碼以 Salt + SHA-256 Hash 加密儲存

### 檔案上傳

- 單檔上限 15 MB
- 檢查副檔名、檔案大小與瀏覽器回傳的 MIME type
- 圖片儲存在 `~/Images`
- 一般檔案儲存在 `~/Files`
- 實際檔名使用 GUID，避免檔名衝突
- 新增、編輯、刪除流程會同步處理資料庫紀錄與實體檔案

## 專案架構

採用ASP.NET MVC 5 架構：

- `Controllers`：前台 Controllers
- `Areas/Admin`：後台 Controllers 與 Views
- `Models`：EF Entity Models
- `Models/ViewModels`：前後台ViewModels
- `DAL`：EF6 `TayanaContext`
- `Migrations`：EF Code First Migrations
- `Methods`：檔案上傳、驗證、密碼雜湊與共用轉換工具
- `Services`：SMTP Email 服務
- `Views`：前台網頁 Razor Views

Routing分為前台與後台：

- 前台：`{controller}/{action}/{id}`
- 後台：`/Admin/{controller}/{action}/{id}`

## 設定

### 環境

- Visual Studio
- .NET Framework 4.8 targeting pack
- SQL Server 或 SQL Server Express
- NuGet package restore

### 配置

`Web.config` 中的值僅作為本機開發範例，不應作為正式環境憑證。

### 資料庫

- `Migrations`中為code-first add-migration code紀錄
- `SqlQuery`為資料庫更新紀錄Scripts

### 網頁入口

- 前台首頁：`/`
- 後台登入：`/Admin/Account/Login`

---

## English Version

**Disclaimer**

**This is not an official website.** This project was built as a backend practice project when attending a 7 months backend dev training (Rocket Camp). It simply focus on ASP.NET MVC 5 backend development, database design, admin CMS workflows, authentication, and file uploads.

## Project Introdutcion
TayanaYachts is an ASP.NET Framework 4.8 MVC 5 project with brand website and a Admin CMS.
- The public site displays homepage, yachts, news, dealers, company information, and contact forms.
- The admin area manages site content (including yacht, news, and delaer), members, and contact inquiries.

## Project Highlights
- Built a public website and admin CMS with ASP.NET MVC 5
- Designed relational data models with Entity Framework 6 Code First
- Implemented admin login and protection with Forms Authentication
- Supported content publishing, file uploads, image management, and physical file cleanup
- Implemented reCAPTCHA validation, contact form persistence, and SMTP email notifications

## Stack

- ASP.NET Framework 4.8, ASP.NET MVC 5, C#
- Entity Framework 6 Code First, SQL Server
- Razor Views, Bootstrap 5, jQuery
- Admin template: AdminLTE
- Content Editor: Summernote WYSIWYG Editor
- Mail Service(for Contact form): MailKit, MimeKit packages
- Authentication: Forms Authentication, Goog=le reCAPTCHA, anti-forgery tokens
- Paging package: PagedList and PagedList.Mvc

## Features

### Public Site

- Homepage: yacht hero image carousel, and 3 latest news
- Yacht: with listing and detail pages
  - Overview, dimensions, and downloads
  - layout / deck plans
  - Specifications
  - Interior images as carousel 
- News: listing and detail pages with pinned news and publish-date filtering
- Dealer: dealer listing grouped by country and area
- Company: Company introduction and certificate pages
- Contact form with validation, reCAPTCHA, database persistence, and email notifications

### Admin CMS

- Secured `Areas/Admin` area using Forms Authentication
- CRUD management for yachts, news, dealers, countries, areas, members, and contact inquiries
- Search, pagination, and publish controls for yachts and news
- Pinned news support
- Contact inquiry completion tracking and soft deletion
- Member passwords stored with per-user salt and SHA-256 hash

### Uploads

- 15 MB file size limit
- File size, extension, and browser-reported MIME type validation
- Images stored in `~/Images`
- General files stored in `~/Files`
- Stored filenames use GUIDs to prevent collisions
- Create, edit, and delete workflows coordinate database records with physical file cleanup

## Architecture
The application follows a traditional ASP.NET MVC 5 structure:

- `Controllers`: public site controllers
- `Areas/Admin`: admin controllers and views
- `Models`: EF entity models
- `Models/ViewModels`: view models
- `DAL`: EF6 `TayanaContext`
- `Migrations`: EF Code First migrations
- `Methods`: file upload, authentication, password hashing, and mapping helpers
- `Services`: SMTP email service
- `Views`: public site Razor views

Routing is split between the public site and admin area:

- Public route: `{controller}/{action}/{id}`
- Admin route: `/Admin/{controller}/{action}/{id}`

## Setup

### Environment

- Visual Studio
- .NET Framework 4.8 targeting pack
- SQL Server or SQL Server Express
- NuGet package restore

### Configuration

`Web.config` values are local development examples and should not be used as production credentials.

### Database

- `Migrations`: code for EF 6 Add-Migration
- `SqlQuery`: SQL Qeury Records for update in MS SQL Server Database

### Entry Points

- Public site: `/`
- Admin login: `/Admin/Account/Login`


