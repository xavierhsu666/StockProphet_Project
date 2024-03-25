using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace StockProphet_Project.Models;


public partial class StocksContext : DbContext
{
    public StocksContext()
    {
    }

    public StocksContext(DbContextOptions<StocksContext> options)
        : base(options)
    {
    }
	public virtual DbSet<DbMember> DbMembers { get; set; }
	public virtual DbSet<DbModel> DbModels { get; set; }

    public virtual DbSet<Stock> Stock { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(local);Database=StockProphet;Integrated Security=true;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
		modelBuilder.Entity<DbMember>(entity =>
		{
			entity.HasKey(e => e.Mid).HasName("PK_Member");

			entity.ToTable("DB_Member");

			entity.Property(e => e.Mid).HasColumnName("MID");
			entity.Property(e => e.MaccoMnt)
				.HasMaxLength(30)
				.HasColumnName("MAccoMnt");
			entity.Property(e => e.Mbirthday).HasColumnName("MBirthday");
			entity.Property(e => e.Memail)
				.HasMaxLength(50)
				.IsUnicode(false)
				.HasColumnName("MEmail");
			entity.Property(e => e.MfavoriteModel)
				.HasMaxLength(50)
				.IsUnicode(false)
				.HasColumnName("MFavoriteModel");
			entity.Property(e => e.Mgender)
				.HasMaxLength(10)
				.IsFixedLength()
				.HasColumnName("MGender");
			entity.Property(e => e.MinvestYear).HasColumnName("MInvestYear");
			entity.Property(e => e.MlastLoginTime).HasColumnName("MLastLoginTime");
			entity.Property(e => e.Mlevel)
				.HasMaxLength(10)
				.IsUnicode(false)
				.HasColumnName("MLevel");
			entity.Property(e => e.Mpassword)
				.HasMaxLength(30)
				.HasColumnName("MPassword");
			entity.Property(e => e.Mprefer).HasColumnName("MPrefer");
			entity.Property(e => e.MregisterTime).HasColumnName("MRegisterTime");
			entity.Property(e => e.MtrueName)
				.HasMaxLength(50)
				.HasColumnName("MTrueName");
		});
		modelBuilder.Entity<DbModel>(entity =>
        {
            entity.HasKey(e => e.Pid).HasName("PK__DB_model__C5775520AB2B0250");

            entity.ToTable("DB_model");

            entity.Property(e => e.Pid).HasColumnName("PID");
            entity.Property(e => e.Dummyblock)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("dummyblock");
            entity.Property(e => e.Paccount)
                .HasMaxLength(30)
                .HasColumnName("PAccount");
            entity.Property(e => e.PbulidTime).HasColumnName("PBulidTime");
            entity.Property(e => e.PfinishTime).HasColumnName("PFinishTime");
            entity.Property(e => e.Plabel)
                .HasColumnType("decimal(7, 2)")
                .HasColumnName("PLabel");
            entity.Property(e => e.Pprefer).HasColumnName("PPrefer");
            entity.Property(e => e.Pstatus)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("PStatus");
            entity.Property(e => e.Pstock)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("PStock");
            entity.Property(e => e.Pvariable)
                .HasMaxLength(100)
                .HasColumnName("PVariable");
        });

        modelBuilder.Entity<Stock>(entity =>
        {

            entity
                .HasKey(e => e.SPk);
			entity.ToTable("Stock");
			entity.Property(e => e.SPk)
                .HasMaxLength(50)
                .HasColumnName("S_PK");
            entity.Property(e => e.SbBussinessIncome).HasColumnName("SB_BussinessIncome");
            entity.Property(e => e.SbEps).HasColumnName("SB_EPS");
            entity.Property(e => e.SbPbratio).HasColumnName("SB_PBRatio");
            entity.Property(e => e.SbYield)
                .HasColumnType("decimal(10, 7)")
                .HasColumnName("SB_Yield");
            entity.Property(e => e.SiD30).HasColumnName("SI_D_30");
            entity.Property(e => e.SiD5).HasColumnName("SI_D_5");
            entity.Property(e => e.SiDif).HasColumnName("SI_Dif");
            entity.Property(e => e.SiK30).HasColumnName("SI_K_30");
            entity.Property(e => e.SiK5).HasColumnName("SI_K_5");
            entity.Property(e => e.SiLongEma).HasColumnName("SI_LongEMA");
            entity.Property(e => e.SiMa).HasColumnName("SI_MA");
            entity.Property(e => e.SiMacd).HasColumnName("SI_MACD");
            entity.Property(e => e.SiMovingAverage30).HasColumnName("SI_MovingAverage_30");
            entity.Property(e => e.SiMovingAverage5).HasColumnName("SI_MovingAverage_5");
            entity.Property(e => e.SiOsc).HasColumnName("SI_OSC");
            entity.Property(e => e.SiPe).HasColumnName("SI_PE");
            entity.Property(e => e.SiRsv30).HasColumnName("SI_RSV_30");
            entity.Property(e => e.SiRsv5).HasColumnName("SI_RSV_5");
            entity.Property(e => e.SiShortEma).HasColumnName("SI_ShortEMA");
            entity.Property(e => e.SnCode)
                .HasMaxLength(10)
                .HasColumnName("SN_Code");
            entity.Property(e => e.SnName)
                .HasMaxLength(50)
                .HasColumnName("SN_Name");
            entity.Property(e => e.StDate).HasColumnName("ST_Date");
            entity.Property(e => e.StQuarter)
                .HasMaxLength(20)
                .HasColumnName("ST_Quarter");
            entity.Property(e => e.StYear)
                .HasMaxLength(20)
                .HasColumnName("ST_Year");
            entity.Property(e => e.StYearQuarter)
                .HasMaxLength(20)
                .HasColumnName("ST_Year_Quarter");
            entity.Property(e => e.SteClose)
                .HasColumnType("decimal(7, 2)")
                .HasColumnName("STe_Close");
            entity.Property(e => e.SteDividendYear).HasColumnName("STe_Dividend_Year");
            entity.Property(e => e.SteMax)
                .HasColumnType("decimal(7, 2)")
                .HasColumnName("STe_Max");
            entity.Property(e => e.SteMin)
                .HasColumnType("decimal(7, 2)")
                .HasColumnName("STe_Min");
            entity.Property(e => e.SteOpen)
                .HasColumnType("decimal(7, 2)")
                .HasColumnName("STe_Open");
            entity.Property(e => e.SteSpreadRatio).HasColumnName("STe_SpreadRatio");
            entity.Property(e => e.SteTradeMoney).HasColumnName("STe_TradeMoney");
            entity.Property(e => e.SteTradeQuantity).HasColumnName("STe_TradeQuantity");
            entity.Property(e => e.SteTransActions).HasColumnName("STe_TransActions");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
