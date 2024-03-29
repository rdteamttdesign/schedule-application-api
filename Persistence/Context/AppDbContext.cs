﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SchedulingTool.Api.Domain.Models;
using Task = SchedulingTool.Api.Domain.Models.Task;

namespace SchedulingTool.Api.Persistence.Context
{
    public partial class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<ColorDef> ColorDefs { get; set; } = null!;
        public virtual DbSet<ColorType> ColorTypes { get; set; } = null!;
        public virtual DbSet<GroupTask> GroupTasks { get; set; } = null!;
        public virtual DbSet<Predecessor> Predecessors { get; set; } = null!;
        public virtual DbSet<PredecessorType> PredecessorTypes { get; set; } = null!;
        public virtual DbSet<Project> Projects { get; set; } = null!;
        public virtual DbSet<ProjectBackground> ProjectBackgrounds { get; set; } = null!;
        public virtual DbSet<ProjectSetting> ProjectSettings { get; set; } = null!;
        public virtual DbSet<Stepwork> Stepworks { get; set; } = null!;
        public virtual DbSet<Task> Tasks { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<View> Views { get; set; } = null!;
        public virtual DbSet<ViewTask> ViewTasks { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("utf8mb4_0900_ai_ci")
                .HasCharSet("utf8mb4");

            modelBuilder.Entity<ColorDef>(entity =>
            {
                entity.HasKey(e => e.ColorId)
                    .HasName("PRIMARY");

                entity.ToTable("color_def");

                entity.HasIndex(e => e.ColorId, "chart_background_id_UNIQUE")
                    .IsUnique();

                entity.HasIndex(e => e.ProjectId, "fk_color_def_project_idx");

                entity.HasIndex(e => e.Type, "fk_color_def_type_idx");

                entity.HasIndex(e => e.Name, "name_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.ColorId).HasColumnName("color_id");

                entity.Property(e => e.Code)
                    .HasMaxLength(45)
                    .HasColumnName("code");

                entity.Property(e => e.IsDefault)
                    .HasColumnType("bit(1)")
                    .HasColumnName("is_default")
                    .HasDefaultValueSql("b'0'");

                entity.Property(e => e.Name)
                    .HasMaxLength(125)
                    .HasColumnName("name");

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.ColorDefs)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_color_def_project");

                entity.HasOne(d => d.TypeNavigation)
                    .WithMany(p => p.ColorDefs)
                    .HasForeignKey(d => d.Type)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_color_def_type");
            });

            modelBuilder.Entity<ColorType>(entity =>
            {
                entity.ToTable("color_type");

                entity.HasIndex(e => e.ColorTypeId, "color_type_id_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.ColorTypeId).HasColumnName("color_type_id");

                entity.Property(e => e.Name)
                    .HasMaxLength(45)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<GroupTask>(entity =>
            {
                entity.ToTable("group_task");

                entity.HasIndex(e => e.ProjectId, "fk_group_task_project_idx");

                entity.HasIndex(e => e.GroupTaskId, "group_task_id_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.GroupTaskId).HasColumnName("group_task_id");

                entity.Property(e => e.GroupTaskName)
                    .HasMaxLength(125)
                    .HasColumnName("group_task_name");

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.GroupTasks)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_group_task_project");
            });

            modelBuilder.Entity<Predecessor>(entity =>
            {
                entity.HasKey(e => new { e.StepworkId, e.RelatedStepworkId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.ToTable("predecessor");

                entity.HasIndex(e => e.RelatedStepworkId, "fk_predecessor_related_step_work_idx");

                entity.HasIndex(e => e.StepworkId, "fk_predecessor_step_work_idx");

                entity.HasIndex(e => e.Type, "fk_predecessor_type_idx");

                entity.Property(e => e.StepworkId).HasColumnName("stepwork_id");

                entity.Property(e => e.RelatedStepworkId).HasColumnName("related_stepwork_id");

                entity.Property(e => e.Lag).HasColumnName("lag");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.HasOne(d => d.RelatedStepwork)
                    .WithMany(p => p.PredecessorRelatedStepworks)
                    .HasForeignKey(d => d.RelatedStepworkId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_predecessor_related_step_work");

                entity.HasOne(d => d.Stepwork)
                    .WithMany(p => p.PredecessorStepworks)
                    .HasForeignKey(d => d.StepworkId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_predecessor_step_work");

                entity.HasOne(d => d.TypeNavigation)
                    .WithMany(p => p.Predecessors)
                    .HasForeignKey(d => d.Type)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_predecessor_type");
            });

            modelBuilder.Entity<PredecessorType>(entity =>
            {
                entity.ToTable("predecessor_type");

                entity.HasIndex(e => e.PredecessorTypeId, "idpredecessor_type_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.PredecessorTypeId).HasColumnName("predecessor_type_id");

                entity.Property(e => e.Description)
                    .HasMaxLength(45)
                    .HasColumnName("description");

                entity.Property(e => e.Name)
                    .HasMaxLength(45)
                    .HasColumnName("name");
            });

            modelBuilder.Entity<Project>(entity =>
            {
                entity.ToTable("project");

                entity.HasIndex(e => e.ProjectName, "project_name_UNIQUE")
                    .IsUnique();

                entity.HasIndex(e => e.ProjectId, "sheet_id_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("created_date");

                entity.Property(e => e.IsActivated)
                    .HasColumnType("bit(1)")
                    .HasColumnName("is_activated")
                    .HasDefaultValueSql("b'1'");

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("modified_date");

                entity.Property(e => e.NumberOfMonths)
                    .HasColumnName("number_of_months")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.ProjectName)
                    .HasMaxLength(125)
                    .HasColumnName("project_name");

                entity.Property(e => e.UserId).HasColumnName("user_id");
            });

            modelBuilder.Entity<ProjectBackground>(entity =>
            {
                entity.HasKey(e => new { e.ProjectId, e.Month })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.ToTable("project_background");

                entity.HasIndex(e => e.ColorId, "fk_proj_bg_bg_idx");

                entity.HasIndex(e => e.ProjectId, "fk_proj_bg_proj_idx");

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.Property(e => e.Month).HasColumnName("month");

                entity.Property(e => e.ColorId).HasColumnName("color_id");

                entity.HasOne(d => d.Color)
                    .WithMany(p => p.ProjectBackgrounds)
                    .HasForeignKey(d => d.ColorId)
                    .HasConstraintName("fk_proj_bg_color");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.ProjectBackgrounds)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_proj_bg_proj");
            });

            modelBuilder.Entity<ProjectSetting>(entity =>
            {
                entity.ToTable("project_setting");

                entity.HasIndex(e => e.ProjectId, "fk_project_setting_project_idx")
                    .IsUnique();

                entity.HasIndex(e => e.ProjectSettingId, "project_setting_id_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.ProjectSettingId).HasColumnName("project_setting_id");

                entity.Property(e => e.AssemblyDurationRatio)
                    .HasColumnName("assembly_duration_ratio")
                    .HasDefaultValueSql("'0.4'");

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.Property(e => e.RemovalDurationRatio)
                    .HasColumnName("removal_duration_ratio")
                    .HasDefaultValueSql("'0.6'");

                entity.Property(e => e.SeparateGroupTask)
                    .HasColumnType("bit(1)")
                    .HasColumnName("separate_group_task")
                    .HasDefaultValueSql("b'0'");

                entity.HasOne(d => d.Project)
                    .WithOne(p => p.ProjectSetting)
                    .HasForeignKey<ProjectSetting>(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_project_setting_project");
            });

            modelBuilder.Entity<Stepwork>(entity =>
            {
                entity.ToTable("stepwork");

                entity.HasIndex(e => e.ColorId, "fk_step_work_color_idx");

                entity.HasIndex(e => e.TaskId, "fk_step_work_task_idx");

                entity.HasIndex(e => e.StepWorkId, "step_work_id_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.StepWorkId).HasColumnName("step_work_id");

                entity.Property(e => e.ColorId).HasColumnName("color_id");

                entity.Property(e => e.Duration).HasColumnName("duration");

                entity.Property(e => e.TaskId).HasColumnName("task_id");

                entity.HasOne(d => d.Color)
                    .WithMany(p => p.Stepworks)
                    .HasForeignKey(d => d.ColorId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_step_work_color");

                entity.HasOne(d => d.Task)
                    .WithMany(p => p.Stepworks)
                    .HasForeignKey(d => d.TaskId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_step_work_task");
            });

            modelBuilder.Entity<Task>(entity =>
            {
                entity.ToTable("task");

                entity.HasIndex(e => e.GroupTaskId, "fk_task_group_task_idx");

                entity.HasIndex(e => e.TaskId, "task_id_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.TaskId).HasColumnName("task_id");

                entity.Property(e => e.AmplifiedDuration).HasColumnName("amplified_duration");

                entity.Property(e => e.Description)
                    .HasMaxLength(125)
                    .HasColumnName("description");

                entity.Property(e => e.Duration).HasColumnName("duration");

                entity.Property(e => e.GroupTaskId).HasColumnName("group_task_id");

                entity.Property(e => e.Index).HasColumnName("index");

                entity.Property(e => e.Note)
                    .HasMaxLength(125)
                    .HasColumnName("note");

                entity.Property(e => e.NumberOfTeam)
                    .HasColumnName("number_of_team")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.TaskName)
                    .HasMaxLength(125)
                    .HasColumnName("task_name");

                entity.HasOne(d => d.GroupTask)
                    .WithMany(p => p.Tasks)
                    .HasForeignKey(d => d.GroupTaskId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_task_group_task");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");

                entity.HasIndex(e => e.UserId, "user_id_UNIQUE")
                    .IsUnique();

                entity.Property(e => e.UserId).HasColumnName("user_id");

                entity.Property(e => e.Avatar)
                    .HasMaxLength(500)
                    .HasColumnName("avatar");

                entity.Property(e => e.CreatedBy).HasColumnName("created_by");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasColumnName("created_date");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(50)
                    .HasColumnName("first_name");

                entity.Property(e => e.IsOnline)
                    .HasColumnType("bit(1)")
                    .HasColumnName("is_online")
                    .HasDefaultValueSql("b'0'");

                entity.Property(e => e.LastName)
                    .HasMaxLength(50)
                    .HasColumnName("last_name");

                entity.Property(e => e.Password)
                    .HasMaxLength(800)
                    .HasColumnName("password");

                entity.Property(e => e.UserName)
                    .HasMaxLength(50)
                    .HasColumnName("user_name");
            });

            modelBuilder.Entity<View>(entity =>
            {
                entity.ToTable("view");

                entity.HasIndex(e => e.ProjectId, "fk_view_project_idx");

                entity.Property(e => e.ViewId).HasColumnName("view_id");

                entity.Property(e => e.ProjectId).HasColumnName("project_id");

                entity.Property(e => e.ViewName)
                    .HasMaxLength(125)
                    .HasColumnName("view_name");

                entity.HasOne(d => d.Project)
                    .WithMany(p => p.Views)
                    .HasForeignKey(d => d.ProjectId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_view_project");
            });

            modelBuilder.Entity<ViewTask>(entity =>
            {
                entity.HasKey(e => new { e.ViewId, e.TaskId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.ToTable("view_task");

                entity.HasIndex(e => e.TaskId, "fk_view_task_task_idx");

                entity.HasIndex(e => e.ViewId, "fk_view_task_view_idx");

                entity.Property(e => e.ViewId).HasColumnName("view_id");

                entity.Property(e => e.TaskId).HasColumnName("task_id");

                entity.Property(e => e.Group).HasColumnName("group");

                entity.HasOne(d => d.Task)
                    .WithMany(p => p.ViewTasks)
                    .HasForeignKey(d => d.TaskId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_view_task_task");

                entity.HasOne(d => d.View)
                    .WithMany(p => p.ViewTasks)
                    .HasForeignKey(d => d.ViewId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_view_task_view");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
