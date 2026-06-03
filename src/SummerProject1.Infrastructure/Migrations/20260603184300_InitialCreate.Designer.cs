using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SummerProject1.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(Microsoft.EntityFrameworkCore.ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            Microsoft.EntityFrameworkCore.SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("SummerProject1.Infrastructure.Entities.Check", b =>
                {
                    b.Property<System.Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<long>("AmountCents")
                        .HasColumnType("bigint");

                    b.Property<System.DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Currency")
                        .IsRequired()
                        .HasMaxLength(3)
                        .HasColumnType("nvarchar(3)");

                    b.Property<System.DateTimeOffset?>("FinishedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Mode")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<System.DateTimeOffset?>("PaidAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("SingleSourceId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<string>("StripeCheckoutSessionId")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("StripePaymentIntentId")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Vin")
                        .IsRequired()
                        .HasMaxLength(17)
                        .HasColumnType("nvarchar(17)");

                    b.HasKey("Id");

                    b.ToTable("Checks");
                });

            modelBuilder.Entity("SummerProject1.Infrastructure.Entities.CheckSource", b =>
                {
                    b.Property<System.Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<System.Guid>("CheckId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<System.DateTimeOffset?>("FinishedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("SourceId")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("nvarchar(40)");

                    b.Property<System.DateTimeOffset?>("StartedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("Id");

                    b.HasIndex("CheckId");

                    b.HasIndex("CheckId", "SourceId")
                        .IsUnique();

                    b.ToTable("CheckSources");
                });

            modelBuilder.Entity("SummerProject1.Infrastructure.Entities.MileageEvent", b =>
                {
                    b.Property<System.Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<System.Guid>("CheckId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Confidence")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("Country")
                        .HasMaxLength(2)
                        .HasColumnType("nvarchar(2)");

                    b.Property<string>("EvidenceUrl")
                        .HasColumnType("nvarchar(max)");

                    b.Property<System.DateTimeOffset>("EventDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<int?>("MileageKm")
                        .HasColumnType("int");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Provider")
                        .IsRequired()
                        .HasMaxLength(60)
                        .HasColumnType("nvarchar(60)");

                    b.Property<string>("SourceId")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("nvarchar(40)");

                    b.HasKey("Id");

                    b.HasIndex("CheckId", "SourceId");

                    b.ToTable("MileageEvents");
                });

            modelBuilder.Entity("SummerProject1.Infrastructure.Entities.SourceResult", b =>
                {
                    b.Property<System.Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<System.Guid>("CheckId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<System.DateTimeOffset>("CreatedAt")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Provider")
                        .IsRequired()
                        .HasMaxLength(60)
                        .HasColumnType("nvarchar(60)");

                    b.Property<string>("RawJson")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SourceId")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("nvarchar(40)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("Id");

                    b.HasIndex("CheckId", "SourceId", "Provider")
                        .IsUnique();

                    b.ToTable("SourceResults");
                });

            modelBuilder.Entity("SummerProject1.Infrastructure.Entities.CheckSource", b =>
                {
                    b.HasOne("SummerProject1.Infrastructure.Entities.Check", null)
                        .WithMany("Sources")
                        .HasForeignKey("CheckId")
                        .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
