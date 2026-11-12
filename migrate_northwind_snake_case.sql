-- Run once against an existing Northwind database before using the updated app.
BEGIN;

ALTER TABLE northwind."Categories" RENAME TO categories;
ALTER TABLE northwind.categories RENAME COLUMN "CategoryID" TO category_id;
ALTER TABLE northwind.categories RENAME COLUMN "CategoryName" TO category_name;
ALTER TABLE northwind.categories RENAME COLUMN "Description" TO description;
ALTER TABLE northwind.categories RENAME COLUMN "Picture" TO picture;

ALTER TABLE northwind."CustomerCustomerDemo" RENAME TO customer_customer_demo;
ALTER TABLE northwind.customer_customer_demo RENAME COLUMN "CustomerID" TO customer_id;
ALTER TABLE northwind.customer_customer_demo RENAME COLUMN "CustomerTypeID" TO customer_type_id;

ALTER TABLE northwind."CustomerDemographics" RENAME TO customer_demographics;
ALTER TABLE northwind.customer_demographics RENAME COLUMN "CustomerTypeID" TO customer_type_id;
ALTER TABLE northwind.customer_demographics RENAME COLUMN "CustomerDesc" TO customer_desc;

ALTER TABLE northwind."Customers" RENAME TO customers;
ALTER TABLE northwind.customers RENAME COLUMN "CustomerID" TO customer_id;
ALTER TABLE northwind.customers RENAME COLUMN "CompanyName" TO company_name;
ALTER TABLE northwind.customers RENAME COLUMN "ContactName" TO contact_name;
ALTER TABLE northwind.customers RENAME COLUMN "ContactTitle" TO contact_title;
ALTER TABLE northwind.customers RENAME COLUMN "Address" TO address;
ALTER TABLE northwind.customers RENAME COLUMN "City" TO city;
ALTER TABLE northwind.customers RENAME COLUMN "Region" TO region;
ALTER TABLE northwind.customers RENAME COLUMN "PostalCode" TO postal_code;
ALTER TABLE northwind.customers RENAME COLUMN "Country" TO country;
ALTER TABLE northwind.customers RENAME COLUMN "Phone" TO phone;
ALTER TABLE northwind.customers RENAME COLUMN "Fax" TO fax;

ALTER TABLE northwind."EmployeeTerritories" RENAME TO employee_territories;
ALTER TABLE northwind.employee_territories RENAME COLUMN "EmployeeID" TO employee_id;
ALTER TABLE northwind.employee_territories RENAME COLUMN "TerritoryID" TO territory_id;

ALTER TABLE northwind."Employees" RENAME TO employees;
ALTER TABLE northwind.employees RENAME COLUMN "EmployeeID" TO employee_id;
ALTER TABLE northwind.employees RENAME COLUMN "LastName" TO last_name;
ALTER TABLE northwind.employees RENAME COLUMN "FirstName" TO first_name;
ALTER TABLE northwind.employees RENAME COLUMN "Title" TO title;
ALTER TABLE northwind.employees RENAME COLUMN "TitleOfCourtesy" TO title_of_courtesy;
ALTER TABLE northwind.employees RENAME COLUMN "BirthDate" TO birth_date;
ALTER TABLE northwind.employees RENAME COLUMN "HireDate" TO hire_date;
ALTER TABLE northwind.employees RENAME COLUMN "Address" TO address;
ALTER TABLE northwind.employees RENAME COLUMN "City" TO city;
ALTER TABLE northwind.employees RENAME COLUMN "Region" TO region;
ALTER TABLE northwind.employees RENAME COLUMN "PostalCode" TO postal_code;
ALTER TABLE northwind.employees RENAME COLUMN "Country" TO country;
ALTER TABLE northwind.employees RENAME COLUMN "HomePhone" TO home_phone;
ALTER TABLE northwind.employees RENAME COLUMN "Extension" TO extension;
ALTER TABLE northwind.employees RENAME COLUMN "Photo" TO photo;
ALTER TABLE northwind.employees RENAME COLUMN "Notes" TO notes;
ALTER TABLE northwind.employees RENAME COLUMN "ReportsTo" TO reports_to;
ALTER TABLE northwind.employees RENAME COLUMN "PhotoPath" TO photo_path;

ALTER TABLE northwind."Order Details" RENAME TO order_details;
ALTER TABLE northwind.order_details RENAME COLUMN "OrderID" TO order_id;
ALTER TABLE northwind.order_details RENAME COLUMN "ProductID" TO product_id;
ALTER TABLE northwind.order_details RENAME COLUMN "UnitPrice" TO unit_price;
ALTER TABLE northwind.order_details RENAME COLUMN "Quantity" TO quantity;
ALTER TABLE northwind.order_details RENAME COLUMN "Discount" TO discount;

ALTER TABLE northwind."Orders" RENAME TO orders;
ALTER TABLE northwind.orders RENAME COLUMN "OrderID" TO order_id;
ALTER TABLE northwind.orders RENAME COLUMN "CustomerID" TO customer_id;
ALTER TABLE northwind.orders RENAME COLUMN "EmployeeID" TO employee_id;
ALTER TABLE northwind.orders RENAME COLUMN "OrderDate" TO order_date;
ALTER TABLE northwind.orders RENAME COLUMN "RequiredDate" TO required_date;
ALTER TABLE northwind.orders RENAME COLUMN "ShippedDate" TO shipped_date;
ALTER TABLE northwind.orders RENAME COLUMN "ShipVia" TO shipper_id;
ALTER TABLE northwind.orders RENAME COLUMN "Freight" TO freight;
ALTER TABLE northwind.orders RENAME COLUMN "ShipName" TO ship_name;
ALTER TABLE northwind.orders RENAME COLUMN "ShipAddress" TO ship_address;
ALTER TABLE northwind.orders RENAME COLUMN "ShipCity" TO ship_city;
ALTER TABLE northwind.orders RENAME COLUMN "ShipRegion" TO ship_region;
ALTER TABLE northwind.orders RENAME COLUMN "ShipPostalCode" TO ship_postal_code;
ALTER TABLE northwind.orders RENAME COLUMN "ShipCountry" TO ship_country;

ALTER TABLE northwind."Products" RENAME TO products;
ALTER TABLE northwind.products RENAME COLUMN "ProductID" TO product_id;
ALTER TABLE northwind.products RENAME COLUMN "ProductName" TO product_name;
ALTER TABLE northwind.products RENAME COLUMN "SupplierID" TO supplier_id;
ALTER TABLE northwind.products RENAME COLUMN "CategoryID" TO category_id;
ALTER TABLE northwind.products RENAME COLUMN "QuantityPerUnit" TO quantity_per_unit;
ALTER TABLE northwind.products RENAME COLUMN "UnitPrice" TO unit_price;
ALTER TABLE northwind.products RENAME COLUMN "UnitsInStock" TO units_in_stock;
ALTER TABLE northwind.products RENAME COLUMN "UnitsOnOrder" TO units_on_order;
ALTER TABLE northwind.products RENAME COLUMN "ReorderLevel" TO reorder_level;
ALTER TABLE northwind.products RENAME COLUMN "Discontinued" TO discontinued;

ALTER TABLE northwind."Regions" RENAME TO regions;
ALTER TABLE northwind.regions RENAME COLUMN "RegionID" TO region_id;
ALTER TABLE northwind.regions RENAME COLUMN "RegionDescription" TO region_description;

ALTER TABLE northwind."Shippers" RENAME TO shippers;
ALTER TABLE northwind.shippers RENAME COLUMN "ShipperID" TO shipper_id;
ALTER TABLE northwind.shippers RENAME COLUMN "CompanyName" TO company_name;
ALTER TABLE northwind.shippers RENAME COLUMN "Phone" TO phone;

ALTER TABLE northwind."Suppliers" RENAME TO suppliers;
ALTER TABLE northwind.suppliers RENAME COLUMN "SupplierID" TO supplier_id;
ALTER TABLE northwind.suppliers RENAME COLUMN "CompanyName" TO company_name;
ALTER TABLE northwind.suppliers RENAME COLUMN "ContactName" TO contact_name;
ALTER TABLE northwind.suppliers RENAME COLUMN "ContactTitle" TO contact_title;
ALTER TABLE northwind.suppliers RENAME COLUMN "Address" TO address;
ALTER TABLE northwind.suppliers RENAME COLUMN "City" TO city;
ALTER TABLE northwind.suppliers RENAME COLUMN "Region" TO region;
ALTER TABLE northwind.suppliers RENAME COLUMN "PostalCode" TO postal_code;
ALTER TABLE northwind.suppliers RENAME COLUMN "Country" TO country;
ALTER TABLE northwind.suppliers RENAME COLUMN "Phone" TO phone;
ALTER TABLE northwind.suppliers RENAME COLUMN "Fax" TO fax;
ALTER TABLE northwind.suppliers RENAME COLUMN "HomePage" TO home_page;

ALTER TABLE northwind."Territories" RENAME TO territories;
ALTER TABLE northwind.territories RENAME COLUMN "TerritoryID" TO territory_id;
ALTER TABLE northwind.territories RENAME COLUMN "TerritoryDescription" TO territory_description;
ALTER TABLE northwind.territories RENAME COLUMN "RegionID" TO region_id;

COMMIT;
