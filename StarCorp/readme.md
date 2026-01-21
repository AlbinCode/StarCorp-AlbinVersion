

### Context
The client has requested a simple E-Commerce API where they can manage their products and allow customers to submit orders. The purpose of these tasks are to gauge your 
understanding of common web API development priciples and design patterns.
For example SOLID programming, dependency injection, object-oriented programming and REST.

### Tasks:
1. Complete the ProductsController so the users can safely create new products, update and delete existing products. e.g Create/Read/Update/Delete (CRUD)
2. Add a new GET endpoint so customers can search for products. You should think about the following
    a. Searching on multiple fields.
	b. Pagination.

#### Caveats:
It is not required for you to complete all tasks but please give each of them your best attempt.

### Requirements:
1. The client **MUST** be able to update product information via the API.
2. API endpoints **SHOULD** follow the RESTful design pattern. 
3. Product stock count **SHOULD** never go below 0.

### What is provided:
We have provided you with an ASP.NET WebAPI project with few intial files. 
1. ProductController with a GET endpoint
2. Data service interfaces; `IProductDataService` and `IOrderDataService`   
3. Interfaces for the data models. `IProduct`, `IOrder` and `IOrderLine`. 

You are expected to use the interfaces provided to complete your tasks. These interfaces may be used by external code. 
If you determine the interfaces need to be changed ensure any changes are backwards compatible with the existing interface. 

You will also find a products.csv file that contains a small selection of sample products for you to begin with. This file is set to copy to the output directory on rebuild.

#### Delivery:
Please host your solution on github and provide us with a link. 

#### Notes:
You are free to make any additional changes to the solution but be prepared to explain and/or demo these changes.      
As long as any changes do not conflict with the requirements listed above.

		