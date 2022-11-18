using BulkyBook.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            CategoryRepository = new CategoryRepository(db);
            CoverTypeRepository = new CoverTypeRepository(db);
            ProductRepository = new ProductRepository(db);
            CompanyRepository = new CompanyRepository(db);  
            ApplicationUserRepository = new ApplicationUserRepository(db);
            ShoppingCartRepository = new ShoppingCartRepository(db);
            OrderHeaderRepository = new OrderHeaderRepository(db);
            OrderDetailRepository = new OrderDetailRepository(db); 
        }
        public ICategoryRepository CategoryRepository { get; private set; }
        public ICoverTypeRepository CoverTypeRepository { get; private set; }
        public IProductRepository ProductRepository { get; private set; }
        public ICompanyRepository CompanyRepository { get; private set; }
        public IShoppingCartRepository ShoppingCartRepository { get; private set; } 
        public IApplicationUserRepository ApplicationUserRepository { get; private set; }
        public IOrderHeaderRepository OrderHeaderRepository { get; private set; }   
        public IOrderDetailRepository OrderDetailRepository { get; private set; }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
