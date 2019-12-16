using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity.Internal;

namespace EfMega
{
    public class Princess : IPerson
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Unicorn> Unicorns { get; set; }
        public virtual ICollection<LadyInWaiting> LadiesInWaiting { get; set; }
    }

    public class LadyInWaiting: IPerson
    {
        [Key, Column(Order = 0)]
        public int PrincessId { get; set; } //Fk for Princess reference

        [Key, Column(Order = 1)]
        public string CastleName { get; set; } //Fk for Castle reference

        public string FirstName { get; set; }
        public string Title { get; set; }


        [NotMapped]
        public string Name => String.Format("{0} {1}", Title, FirstName);

        public virtual Castle Castle { get; set; }
        public virtual Princess Princess { get; set; }
    }


    public class Unicorn
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [Timestamp]
        public byte[] Version { get; set; }

        public int PrincessId { get; set; } // FK for Princess reference
        public virtual Princess Princess { get; set; }
    }

    public class Castle
    {
        [Key]
        public string Name { get; set; }

        public Location Location { get; set; }

        public virtual ICollection<LadyInWaiting> LadiesInWaiting { get; set; }
    }

    [ComplexType]
    public class Location
    {
        public string City { get; set; }
        public string Kingdom { get; set; }

        public ImaginaryWorld ImaginaryWorld { get; set; }
    }

    [ComplexType]
    public class ImaginaryWorld
    {
        public string Name { get; set; }
        public string Creator { get; set; }
    }


    public interface IPerson
    {       
        string Name { get; }
    }


    public class UnicornsContextInitializer : DropCreateDatabaseAlways<UnicornsContext>
    {
        protected override void Seed(UnicornsContext context)
        {
            var cinderella = new Princess { Name = "Cinderella" };
            var sleepingBeauty = new Princess { Name = "Sleeping Beauty" };
            var snowWhite = new Princess { Name = "Snow White" };



            new List<Unicorn>
            {
                new Unicorn { Name = "Binky" , Princess = cinderella },
                new Unicorn { Name = "Silly" , Princess = cinderella },
                new Unicorn { Name = "Beepy" , Princess = sleepingBeauty },
                new Unicorn { Name = "Creepy" , Princess = snowWhite }
            }.ForEach(u => context.Unicorns.Add(u));

            var efCastle = new Castle
            {
                Name = "The EF Castle",
                Location = new Location
                {
                    City = "Redmond",
                    Kingdom = "Rainier",
                    ImaginaryWorld = new ImaginaryWorld
                    {
                        Name = "Magic Unicorn World",
                        Creator = "ADO.NET"
                    }
                },
            };

            new List<LadyInWaiting>
            {
                new LadyInWaiting { Princess = cinderella,
                                    Castle = efCastle,
                                    FirstName = "Lettice",
                                    Title = "Countess" },
                new LadyInWaiting { Princess = sleepingBeauty,
                                    Castle = efCastle,
                                    FirstName = "Ulrika",
                                    Title = "Lady" },
                new LadyInWaiting { Princess = snowWhite,
                                    Castle = efCastle,
                                    FirstName = "Yolande",
                                    Title = "Duchess" }
            }.ForEach(l => context.LadiesInWaiting.Add(l));

            base.Seed(context);

        }

      
    }


    public class UnicornsContext : DbContext
    {

        public UnicornsContext() : base("name=MegaContext")
        {
            Database.SetInitializer(new UnicornsContextInitializer());
            //this.Configuration.LazyLoadingEnabled = false;
        }
        public DbSet<Unicorn> Unicorns { get; set; }
        public DbSet<Princess> Princesses { get; set; }
        public DbSet<LadyInWaiting> LadiesInWaiting { get; set; }
        public DbSet<Castle> Castles { get; set; }
    }




    class Program
    {
        static void Main(string[] args)
        {
            //Database.SetInitializer(new UnicornsContextInitializer());


            using (var context = new UnicornsContext())
            {
                var princess = context.Princesses.Find(1);

                // Load the unicorns starting with B related to a given princess 
                context.Entry(princess)
                    .Collection(p => p.Unicorns)
                    .Query()
                    .Where(u => u.Name.StartsWith("B"))
                    .Load();
            }

            Console.WriteLine("Done!");
            Console.ReadLine();
        }

        public static void PrintValues(DbPropertyValues values)
        {
            foreach(var propertyName in values.PropertyNames)
            {
                Console.WriteLine("Property {0} has value {1}",
                                            propertyName, values[propertyName]);
            }
        }
  
        


        //static void Print()
        //{
        //    using (var context = new UnicornsContext())
        //    {
        //        string dpath = @"D:\so";
        //        string fpath = dpath + @"\soutput.txt";

        //        if (Directory.Exists(dpath))
        //        {
        //            var contents = context.Database.SqlQuery<Princess>("Select * from Princesses").ToList();
        //            //File.AppendAllLines(fpath,contents);
        //        }

        //        else
        //            Directory.CreateDirectory(dpath);

        //    }
        //}

        
    }
}
