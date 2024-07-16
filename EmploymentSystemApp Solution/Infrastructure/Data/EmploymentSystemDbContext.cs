using Core.Models;
using Microsoft.EntityFrameworkCore;


namespace Infrastructure.Data
{
    public class EmploymentSystemDbContext : DbContext
    {
        public EmploymentSystemDbContext(DbContextOptions<EmploymentSystemDbContext> options) : base(options)
        {
            
        }

        public DbSet<Applicant> Applicants { get; set; }
        public DbSet<Employer> Employers { get; set; }
        public DbSet<Vacancy> Vacancies { get; set; }
        public DbSet<Application> Applications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            //Employer
            modelBuilder.Entity<Employer>(EmployerEntity =>
            {
                EmployerEntity.HasKey(E => E.UserId);
                
                EmployerEntity.Property(E => E.Username)
                              .IsRequired()
                              .HasMaxLength(30);

                EmployerEntity.Property(E => E.Password)
                              .IsRequired();

                EmployerEntity.Property(E => E.Email)
                              .IsRequired();

                EmployerEntity.Property(E => E.Location)
                              .IsRequired();

                EmployerEntity.HasMany(E => E.Vacancies)
                              .WithOne(V => V.Employer)
                              .HasForeignKey(V => V.EmployerId);
            });




            //Applicant
            modelBuilder.Entity<Applicant>(ApplicantEntity =>
            {
                ApplicantEntity
                .HasKey(A => A.UserId);

                ApplicantEntity
                .Property(A => A.Username)
                .IsRequired()
                .HasMaxLength(30);

                ApplicantEntity
                .Property(A => A.Password)
                .IsRequired();

                ApplicantEntity
                .Property(A => A.Email)
                .IsRequired();

                ApplicantEntity
                .Property(A => A.Skills)
                .IsRequired();

                ApplicantEntity
                .Property(A => A.Experience)
                .IsRequired();

               ApplicantEntity
                .HasMany(A => A.Applications)
                .WithOne(AP => AP.Applicant)
                .HasForeignKey(AP => AP.ApplicantId);
            });


            //Vacancy

            modelBuilder.Entity<Vacancy>(VacancyEntity =>
            {
                VacancyEntity
                    .HasKey(V => V.VacancyId);

                VacancyEntity
                    .Property(V => V.Title)
                    .IsRequired()
                    .HasMaxLength(100);

                VacancyEntity
                    .Property(V => V.Description)
                    .IsRequired()
                    .HasMaxLength(100);

                VacancyEntity
                    .Property(V => V.MaxApplications)
                    .IsRequired();

                VacancyEntity
                    .Property(V => V.ExpireDate)
                    .IsRequired();

                

                VacancyEntity
                    .Property(V => V.CreatedDate)
                    .IsRequired();

                VacancyEntity
                    .Property(V => V.IsActive)
                    .IsRequired();

                VacancyEntity
                    .HasMany(V => V.Applications)
                    .WithOne(AP => AP.Vacancy)
                    .HasForeignKey(AP => AP.VacancyId);
                    

            });

            //Applications

            modelBuilder.Entity<Application>(ApplicationEntity => {
                ApplicationEntity.
                    HasKey(a => a.ApplicationId);

                ApplicationEntity
                    .Property(A => A.ApplicationDate).IsRequired();

                ApplicationEntity
                    .HasOne(A => A.Applicant)
                    .WithMany(Ap => Ap.Applications)
                    .HasForeignKey(Ap => Ap.ApplicantId);

                ApplicationEntity
                    .HasOne(A => A.Vacancy)
                    .WithMany(Ap => Ap.Applications)
                    .HasForeignKey(Ap => Ap.VacancyId);
            });
            

            

            

            

            
              

        }
    }
}
