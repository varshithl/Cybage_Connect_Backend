using System;
using System.Collections.Generic;
using Cybage_Connect.Models;
using Microsoft.EntityFrameworkCore;

namespace Cybage_Connect.Data;

public partial class CybageConnectDbContext : DbContext
{
    public CybageConnectDbContext()
    {
    }

    public CybageConnectDbContext(DbContextOptions<CybageConnectDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ArticlesOfUser> ArticlesOfUsers { get; set; }

    public virtual DbSet<BlogsOfUser> BlogsOfUsers { get; set; }

    public virtual DbSet<Comment> Comments { get; set; }

    public virtual DbSet<Connection> Connections { get; set; }

    public virtual DbSet<FriendRequest> FriendRequests { get; set; }

    public virtual DbSet<Like> Likes { get; set; }

    public virtual DbSet<ProjectInsightsOfUser> ProjectInsightsOfUsers { get; set; }

    public virtual DbSet<RequestStorage> RequestStorages { get; set; }

    public virtual DbSet<UserRegistration> UserRegistrations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=GVC1390\\MSSQLSERVER2019;Initial Catalog=CybageConnectDB;User ID=sa;Password=cybage@123456;Encrypt=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ArticlesOfUser>(entity =>
        {
            entity.HasKey(e => e.ArticleId).HasName("PK__Articles__9C6270E88C0FAFE8");

            entity.Property(e => e.ArticleTitle).IsUnicode(false);
            entity.Property(e => e.Comments)
                .HasDefaultValue(0)
                .HasColumnName("comments");
            entity.Property(e => e.Content).IsUnicode(false);
            entity.Property(e => e.Likes).HasDefaultValue(0);
            entity.Property(e => e.PublishedDate).HasColumnType("datetime");
            entity.Property(e => e.UserName)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        modelBuilder.Entity<BlogsOfUser>(entity =>
        {
            entity.HasKey(e => e.BlogId).HasName("PK__BlogsOfU__54379E308055416C");

            entity.Property(e => e.BlogTitle).IsUnicode(false);
            entity.Property(e => e.Comments)
                .HasDefaultValue(0)
                .HasColumnName("comments");
            entity.Property(e => e.Content).IsUnicode(false);
            entity.Property(e => e.Likes).HasDefaultValue(0);
            entity.Property(e => e.PublishedDateOfBlog)
                .HasColumnType("datetime")
                .HasColumnName("PublishedDateOfBLog");
            entity.Property(e => e.UserName)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Comments__3214EC0768C9ED87");

            entity.ToTable(tb => tb.HasTrigger("commentsTrigger"));

            entity.Property(e => e.ArticleId).HasDefaultValue(0);
            entity.Property(e => e.BlogId).HasDefaultValue(0);
            entity.Property(e => e.Comment1)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("Comment");
            entity.Property(e => e.ProjectInsightId).HasDefaultValue(0);
        });

        modelBuilder.Entity<Connection>(entity =>
        {
            entity.HasKey(e => e.ConnectId).HasName("PK__connecti__BC971B9C3FA34D72");

            entity.ToTable("connections");

            entity.HasOne(d => d.User).WithMany(p => p.Connections)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__connectio__UserI__47DBAE45");
        });

        modelBuilder.Entity<FriendRequest>(entity =>
        {
            entity.HasKey(e => e.ReqId).HasName("PK__FriendRe__28A9A382715B0F96");

            entity.Property(e => e.RqstMessage)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SenderId).HasColumnName("senderID");

            entity.HasOne(d => d.Receiver).WithMany(p => p.FriendRequestReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .HasConstraintName("FK__FriendReq__Recei__44FF419A");

            entity.HasOne(d => d.Sender).WithMany(p => p.FriendRequestSenders)
                .HasForeignKey(d => d.SenderId)
                .HasConstraintName("FK__FriendReq__sende__440B1D61");
        });

        modelBuilder.Entity<Like>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Likes__3214EC077BA96F24");

            entity.ToTable(tb =>
                {
                    tb.HasTrigger("UnlikesTrigger");
                    tb.HasTrigger("likesTrigger");
                });

            entity.Property(e => e.ArticleId).HasDefaultValue(0);
            entity.Property(e => e.BlogId).HasDefaultValue(0);
            entity.Property(e => e.ProjectInsightId).HasDefaultValue(0);
        });

        modelBuilder.Entity<ProjectInsightsOfUser>(entity =>
        {
            entity.HasKey(e => e.ProjectInsightId).HasName("PK__ProjectI__AF5AA8B66F528A5F");

            entity.Property(e => e.Comments)
                .HasDefaultValue(0)
                .HasColumnName("comments");
            entity.Property(e => e.Duration)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Likes).HasDefaultValue(0);
            entity.Property(e => e.ProjectDescription).IsUnicode(false);
            entity.Property(e => e.ProjectTitle).IsUnicode(false);
            entity.Property(e => e.PublishedDateOfProjectInsight).HasColumnType("datetime");
            entity.Property(e => e.Tools).IsUnicode(false);
            entity.Property(e => e.UserName)
                .HasMaxLength(30)
                .IsUnicode(false);
        });

        modelBuilder.Entity<RequestStorage>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("PK__Request___E9C5B373C10BE94D");

            entity.ToTable("Request_Storage");

            entity.Property(e => e.RequestId).HasColumnName("Request_Id");
            entity.Property(e => e.ConnectionStatus)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.RequestReceiverId).HasColumnName("Request_ReceiverId");
            entity.Property(e => e.RequestSenderId).HasColumnName("Request_senderID");

            entity.HasOne(d => d.RequestReceiver).WithMany(p => p.RequestStorageRequestReceivers)
                .HasForeignKey(d => d.RequestReceiverId)
                .HasConstraintName("FK__Request_S__Reque__5070F446");

            entity.HasOne(d => d.RequestSender).WithMany(p => p.RequestStorageRequestSenders)
                .HasForeignKey(d => d.RequestSenderId)
                .HasConstraintName("FK__Request_S__Reque__4F7CD00D");
        });

        modelBuilder.Entity<UserRegistration>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__UserRegi__1788CC4CF2C3798B");

            entity.ToTable("UserRegistration", tb => tb.HasTrigger("ConnectionGenerator"));

            entity.HasIndex(e => e.MobileNumber, "UQ__UserRegi__250375B1E495F3A9").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__UserRegi__A9D10534849F0948").IsUnique();

            entity.HasIndex(e => e.UserName, "UQ__UserRegi__C9F28456DE2FE373").IsUnique();

            entity.Property(e => e.Designation)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(300)
                .IsUnicode(false);
            entity.Property(e => e.FullName)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.UserName)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.UserPassword)
                .HasMaxLength(40)
                .IsUnicode(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
