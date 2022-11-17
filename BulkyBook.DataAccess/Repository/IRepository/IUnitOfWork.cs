﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository.IRepository
{
    //parent wrapper of all repositories we have
    //here are global methods
    public interface IUnitOfWork
    {
        ICategoryRepository CategoryRepository { get; }
        ICoverTypeRepository CoverTypeRepository { get; }
        IProductRepository ProductRepository { get; }
        ICompanyRepository CompanyRepository { get; }
        IApplicationUserRepository ApplicationUserRepository { get; }
        IShoppingCartRepository ShoppingCartRepository { get; }
        void Save();
    }
}
