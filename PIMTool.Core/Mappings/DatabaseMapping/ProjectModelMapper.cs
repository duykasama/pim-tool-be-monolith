using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using PIMTool.Core.Domain.Entities;
using PIMTool.Core.Interfaces;

namespace PIMTool.Core.Mappings.DatabaseMapping;

public class ProjectModelMapper : IModelMapper
{
    public void Mapping(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(entity =>
        {
            // entity.ToTable(nameof(Project));
            //
            // entity.HasKey(e => e.Id);
            //
            // entity.Property(e => e.GroupId)
            //     .IsRequired();
            //
            // entity.Property(e => e.ProjectNumber)
            //     .IsRequired();
            //
            // entity.Property(e => e.Name)
            //     .HasMaxLength(50)
            //     .IsRequired();
            //
            // entity.Property(e => e.Customer)
            //     .HasMaxLength(50)
            //     .IsRequired();
            //
            // entity.Property(e => e.Status)
            //     .HasMaxLength(3)
            //     .IsRequired();
            //
            // entity.Property(e => e.StartDate)
            //     .IsRequired();
            //
            // // entity.Property(e => e.EndDate);
            //
            // entity.Property(e => e.Version)
            //     .IsRequired();
            //
            // // entity.Property(e => e.CreatedAt);
            // //
            // // entity.Ignore(e => e.CreatedBy);
            // //
            // // entity.Ignore(e => e.IsDeleted);
            // //
            // // entity.Ignore(e => e.UpdatedAt);
            // //
            // // entity.Ignore(e => e.UpdatedBy);
            //
            // entity.Property(e => e.Id).HasColumnName(nameof(Project.Id));
            // entity.Property(e => e.GroupId).HasColumnName(nameof(Project.GroupId));
            // entity.Property(e => e.ProjectNumber).HasColumnName(nameof(Project.ProjectNumber));
            // entity.Property(e => e.Name).HasColumnName(nameof(Project.Name));
            // entity.Property(e => e.Customer).HasColumnName(nameof(Project.Customer));
            // entity.Property(e => e.Status).HasColumnName(nameof(Project.Status));
            // entity.Property(e => e.StartDate).HasColumnName(nameof(Project.StartDate));
            // entity.Property(e => e.EndDate).HasColumnName(nameof(Project.EndDate));
            // entity.Property(e => e.Version).HasColumnName(nameof(Project.Version));
            // entity.Property(e => e.CreatedAt).HasColumnName(nameof(Project.CreatedAt));
            // entity.Property(e => e.CreatedBy).HasColumnName(nameof(Project.CreatedBy));
            // entity.Property(e => e.IsDeleted).HasColumnName(nameof(Project.IsDeleted));
            // entity.Property(e => e.UpdatedAt).HasColumnName(nameof(Project.UpdatedAt));
            // entity.Property(e => e.UpdatedBy).HasColumnName(nameof(Project.UpdatedBy));
            
            entity.ToTable(nameof(Project));
            
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())");
            
            entity.Property(e => e.CreatedAt)
                .HasColumnType("date");
            
            entity.Property(e => e.Customer)
                .HasMaxLength(50)
                .IsUnicode(false);
            
            entity.Property(e => e.EndDate)
                .HasColumnType("date");
            
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            
            entity.Property(e => e.StartDate)
                .HasColumnType("date");
            
            entity.Property(e => e.Status)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("date");

            entity.HasOne(d => d.Group).WithMany(p => p.Projects)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasMany(d => d.Employees).WithMany(p => p.Projects)
                .UsingEntity<Dictionary<string, object>>(
                    "ProjectEmployee",
                    r => r.HasOne<Employee>().WithMany()
                        .HasForeignKey("EmployeeId")
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    l => l.HasOne<Project>().WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.ClientSetNull),
                    j =>
                    {
                        j.HasKey("ProjectId", "EmployeeId");
                        j.ToTable("Project_Employee");
                    });
            
        });
    }
}