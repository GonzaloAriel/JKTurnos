# Generar migracion EF6: InitialCreate (regenerada con RecursoId)
# Lee el EDMX original, agrega RecursoId, elimina Cascade en Turno,
# y genera los archivos de migracion completos.

Add-Type -AssemblyName System.IO.Compression

$edmxPath = "C:\Users\Gonzo\Desktop\JKTurnos\Sistema Turnos\JKTurnos\JKTurnos\Docs\current_edmx.xml"
$content = Get-Content $edmxPath -Raw
$contentLen = $content.Length
Write-Output "EDMX length: $contentLen"

# Split en CSDL+MSL y SSDL
$ssdlSplit = $content.IndexOf('Namespace="CodeFirstDatabaseSchema"')
$csdlPart = $content.Substring(0, $ssdlSplit)
$ssdlPart = $content.Substring($ssdlSplit)

Write-Output "SSDL starts at: $ssdlSplit"

# ==================== MODIFICACIONES CSDL ====================

# 1. Agregar RecursoId property + NavigationProperty en Turno EntityType
$oldProp = '<Property Name="EstadoTurnoId" Type="Int32" Nullable="false" />'
$newProp = $oldProp + "`n          <Property Name=""RecursoId"" Type=""Int32"" Nullable=""false"" />`n          <NavigationProperty Name=""Recurso"" Relationship=""Self.Turno_Recurso"" FromRole=""Turno_Recurso_Source"" ToRole=""Turno_Recurso_Target"" />"
$csdlPart = $csdlPart.Replace($oldProp, $newProp)

# 2. Agregar RecursoId en MSL mapping
$oldMsl = '<ScalarProperty Name="EstadoTurnoId" ColumnName="EstadoTurnoId" />'
$newMsl = $oldMsl + "`n                <ScalarProperty Name=""RecursoId"" ColumnName=""RecursoId"" />"
$csdlPart = $csdlPart.Replace($oldMsl, $newMsl)

# 3. Eliminar OnDelete Cascade de Turno_Cliente, Turno_EstadoTurno, Turno_Servicio (CSDL)
$csdlPart = $csdlPart.Replace(
    '          <End Role="Turno_Cliente_Target" Type="Self.Cliente" Multiplicity="1">
            <OnDelete Action="Cascade" />
          </End>',
    '          <End Role="Turno_Cliente_Target" Type="Self.Cliente" Multiplicity="1">
          </End>'
)
$csdlPart = $csdlPart.Replace(
    '          <End Role="Turno_EstadoTurno_Target" Type="Self.EstadoTurno" Multiplicity="1">
            <OnDelete Action="Cascade" />
          </End>',
    '          <End Role="Turno_EstadoTurno_Target" Type="Self.EstadoTurno" Multiplicity="1">
          </End>'
)
$csdlPart = $csdlPart.Replace(
    '          <End Role="Turno_Servicio_Target" Type="Self.Servicio" Multiplicity="1">
            <OnDelete Action="Cascade" />
          </End>',
    '          <End Role="Turno_Servicio_Target" Type="Self.Servicio" Multiplicity="1">
          </End>'
)

# 4. Agregar Association Turno_Recurso en CSDL (sin Cascade)
$csdlLastAssoc = $csdlPart.LastIndexOf('</Association>')
$csdlNewAssoc = @"

        <Association Name="Turno_Recurso">
          <End Role="Turno_Recurso_Source" Type="Self.Turno" Multiplicity="*" />
          <End Role="Turno_Recurso_Target" Type="Self.Recurso" Multiplicity="1">
          </End>
          <ReferentialConstraint>
            <Principal Role="Turno_Recurso_Target">
              <PropertyRef Name="Id" />
            </Principal>
            <Dependent Role="Turno_Recurso_Source">
              <PropertyRef Name="RecursoId" />
            </Dependent>
          </ReferentialConstraint>
        </Association>
"@
$csdlPart = $csdlPart.Insert($csdlLastAssoc + 15, $csdlNewAssoc)

# 5. Agregar AssociationSet Turno_Recurso en CSDL EntityContainer
$csdlUserTenant = $csdlPart.IndexOf('User_Tenant')
$csdlLastAssocSet = $csdlPart.IndexOf('</AssociationSet>', $csdlUserTenant)
$csdlNewAssocSet = @"

          <AssociationSet Name="Turno_Recurso" Association="Self.Turno_Recurso">
            <End Role="Turno_Recurso_Source" EntitySet="Turnos" />
            <End Role="Turno_Recurso_Target" EntitySet="Recursos" />
          </AssociationSet>
"@
$csdlPart = $csdlPart.Insert($csdlLastAssocSet + 17, $csdlNewAssocSet)

# ==================== MODIFICACIONES SSDL ====================

# 6. Agregar RecursoId column en SSDL Turno EntityType
$oldSdlProp = '<Property Name="EstadoTurnoId" Type="Int32" Nullable="false" />'
$newSdlProp = $oldSdlProp + "`n          <Property Name=""RecursoId"" Type=""Int32"" Nullable=""false"" />"
$ssdlPart = $ssdlPart.Replace($oldSdlProp, $newSdlProp)

# 7. Eliminar OnDelete Cascade de Turno associations en SSDL
$ssdlPart = $ssdlPart.Replace(
    '          <End Role="Cliente" Type="Self.Cliente" Multiplicity="1">
            <OnDelete Action="Cascade" />
          </End>
          <End Role="Turno" Type="Self.Turno" Multiplicity="*" />',
    '          <End Role="Cliente" Type="Self.Cliente" Multiplicity="1">
          </End>
          <End Role="Turno" Type="Self.Turno" Multiplicity="*" />'
)
$ssdlPart = $ssdlPart.Replace(
    '          <End Role="EstadoTurno" Type="Self.EstadoTurno" Multiplicity="1">
            <OnDelete Action="Cascade" />
          </End>
          <End Role="Turno" Type="Self.Turno" Multiplicity="*" />',
    '          <End Role="EstadoTurno" Type="Self.EstadoTurno" Multiplicity="1">
          </End>
          <End Role="Turno" Type="Self.Turno" Multiplicity="*" />'
)
$ssdlPart = $ssdlPart.Replace(
    '          <End Role="Servicio" Type="Self.Servicio" Multiplicity="1">
            <OnDelete Action="Cascade" />
          </End>
          <End Role="Turno" Type="Self.Turno" Multiplicity="*" />',
    '          <End Role="Servicio" Type="Self.Servicio" Multiplicity="1">
          </End>
          <End Role="Turno" Type="Self.Turno" Multiplicity="*" />'
)

# 8. Agregar Association Turno_Recurso en SSDL
$ssdlFirstAssoc = $ssdlPart.IndexOf('<Association')
$ssdlAssocEnd = $ssdlFirstAssoc
while ($true) {
    $endClose = $ssdlPart.IndexOf('</Association>', $ssdlFirstAssoc)
    $nextAssoc = $ssdlPart.IndexOf('<Association', $endClose + 15)
    if ($nextAssoc -lt 0) { break }
    $ssdlFirstAssoc = $nextAssoc
    $ssdlAssocEnd = $ssdlPart.IndexOf('</Association>', $ssdlFirstAssoc)
}
if ($ssdlAssocEnd -eq $ssdlFirstAssoc) { $ssdlAssocEnd = $ssdlPart.IndexOf('</Association>') }
$ssdlNewAssoc = @"

        <Association Name="Turno_Recurso">
          <End Role="Turno_Recurso_Source" Type="Self.Turno" Multiplicity="*" />
          <End Role="Turno_Recurso_Target" Type="Self.Recurso" Multiplicity="1">
          </End>
          <ReferentialConstraint>
            <Principal Role="Turno_Recurso_Target">
              <PropertyRef Name="Id" />
            </Principal>
            <Dependent Role="Turno_Recurso_Source">
              <PropertyRef Name="RecursoId" />
            </Dependent>
          </ReferentialConstraint>
        </Association>
"@
$ssdlPart = $ssdlPart.Insert($ssdlAssocEnd + 15, $ssdlNewAssoc)

# 9. Agregar AssociationSet Turno_Recurso en SSDL EntityContainer
$ssdlContainer = $ssdlPart.IndexOf('<EntityContainer')
$ssdlFirstSet = $ssdlPart.IndexOf('</EntitySet>', $ssdlContainer)
$ssdlLastSet = $ssdlFirstSet
while ($true) {
    $nextSet = $ssdlPart.IndexOf('<EntitySet', $ssdlLastSet + 15)
    if ($nextSet -lt 0) { break }
    $ssdlLastSet = $ssdlPart.IndexOf('</EntitySet>', $nextSet)
}
$ssdlNewAssocSet = @"

          <AssociationSet Name="Turno_Recurso" Association="Self.Turno_Recurso">
            <End Role="Turno_Recurso_Source" EntitySet="Turno" />
            <End Role="Turno_Recurso_Target" EntitySet="Recurso" />
          </AssociationSet>
"@
$ssdlPart = $ssdlPart.Insert($ssdlLastSet + 13, $ssdlNewAssocSet)

# ==================== RECOMBINAR Y COMPRIMIR ====================
$content = $csdlPart + $ssdlPart
Write-Output "New EDMX length: $($content.Length)"

$bytesOut = [System.Text.Encoding]::UTF8.GetBytes($content)
$msOut = New-Object IO.MemoryStream
$gzipOut = New-Object IO.Compression.GZipStream($msOut, [IO.Compression.CompressionMode]::Compress)
$gzipOut.Write($bytesOut, 0, $bytesOut.Length)
$gzipOut.Close()
$compressed = $msOut.ToArray()
$msOut.Close()
$newBase64 = [Convert]::ToBase64String($compressed)

Write-Output "New Target base64 length: $($newBase64.Length)"

# ==================== CREAR ARCHIVOS DE MIGRACION ====================
$migrationDir = "C:\Users\Gonzo\Desktop\JKTurnos\Sistema Turnos\JKTurnos\JKTurnos\Migrations"
$timestamp = "20260727200000"
$className = "InitialCreate"

$csContent = @"
namespace JKTurnos.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Clientes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantId = c.Int(nullable: false),
                        Nombre = c.String(nullable: false, maxLength: 120),
                        Telefono = c.String(maxLength: 50),
                        Email = c.String(maxLength: 120),
                        Direccion = c.String(maxLength: 250),
                        Notas = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.ConfiguracionTenants",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantId = c.Int(nullable: false),
                        NombreComercial = c.String(nullable: false, maxLength: 150),
                        Telefono = c.String(maxLength: 50),
                        Email = c.String(maxLength: 150),
                        Direccion = c.String(maxLength: 250),
                        LogoUrl = c.String(maxLength: 500),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Tenants", t => t.TenantId, cascadeDelete: true)
                .Index(t => t.TenantId);

            CreateTable(
                "dbo.Tenants",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Nombre = c.String(),
                        Subdominio = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.EstadoTurnoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantId = c.Int(nullable: false),
                        Nombre = c.String(),
                        Color = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Recursoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantId = c.Int(nullable: false),
                        Nombre = c.String(nullable: false, maxLength: 100),
                        Descripcion = c.String(),
                        Tipo = c.Int(nullable: false),
                        Color = c.String(maxLength: 7),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Servicios",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantId = c.Int(nullable: false),
                        Nombre = c.String(),
                        Descripcion = c.String(),
                        Precio = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DuracionMinutos = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.Turnoes",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TenantId = c.Int(nullable: false),
                        ClienteId = c.Int(nullable: false),
                        ServicioId = c.Int(nullable: false),
                        EstadoTurnoId = c.Int(nullable: false),
                        RecursoId = c.Int(nullable: false),
                        FechaHoraInicio = c.DateTime(nullable: false),
                        FechaHoraFin = c.DateTime(nullable: false),
                        Observaciones = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Clientes", t => t.ClienteId, cascadeDelete: false)
                .ForeignKey("dbo.EstadoTurnoes", t => t.EstadoTurnoId, cascadeDelete: false)
                .ForeignKey("dbo.Servicios", t => t.ServicioId, cascadeDelete: false)
                .ForeignKey("dbo.Recursoes", t => t.RecursoId, cascadeDelete: false)
                .Index(t => t.ClienteId)
                .Index(t => t.ServicioId)
                .Index(t => t.EstadoTurnoId)
                .Index(t => t.RecursoId);

            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Email = c.String(),
                        PasswordHash = c.String(),
                        Nombre = c.String(),
                        Role = c.String(),
                        TenantId = c.Int(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        UpdatedAt = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Tenants", t => t.TenantId, cascadeDelete: true)
                .Index(t => t.TenantId);

        }

        public override void Down()
        {
            DropForeignKey("dbo.Users", "TenantId", "dbo.Tenants");
            DropForeignKey("dbo.Turnoes", "RecursoId", "dbo.Recursoes");
            DropForeignKey("dbo.Turnoes", "ServicioId", "dbo.Servicios");
            DropForeignKey("dbo.Turnoes", "EstadoTurnoId", "dbo.EstadoTurnoes");
            DropForeignKey("dbo.Turnoes", "ClienteId", "dbo.Clientes");
            DropForeignKey("dbo.ConfiguracionTenants", "TenantId", "dbo.Tenants");
            DropIndex("dbo.Users", new[] { "TenantId" });
            DropIndex("dbo.Turnoes", new[] { "RecursoId" });
            DropIndex("dbo.Turnoes", new[] { "EstadoTurnoId" });
            DropIndex("dbo.Turnoes", new[] { "ServicioId" });
            DropIndex("dbo.Turnoes", new[] { "ClienteId" });
            DropIndex("dbo.ConfiguracionTenants", new[] { "TenantId" });
            DropTable("dbo.Users");
            DropTable("dbo.Turnoes");
            DropTable("dbo.Servicios");
            DropTable("dbo.Recursoes");
            DropTable("dbo.EstadoTurnoes");
            DropTable("dbo.Tenants");
            DropTable("dbo.ConfiguracionTenants");
            DropTable("dbo.Clientes");
        }
    }
}
"@
Set-Content -Path "$migrationDir\${timestamp}_${className}.cs" -Value $csContent -Encoding UTF8
Write-Output "Created: ${timestamp}_${className}.cs"

$designerContent = @"
// <auto-generated />
namespace JKTurnos.Migrations
{
    using System.CodeDom.Compiler;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Resources;

    [GeneratedCode("EntityFramework.Migrations", "6.4.4")]
    public sealed partial class InitialCreate : IMigrationMetadata
    {
        private readonly ResourceManager Resources = new ResourceManager(typeof(InitialCreate));

        string IMigrationMetadata.Id
        {
            get { return "${timestamp}_InitialCreate"; }
        }

        string IMigrationMetadata.Source
        {
            get { return null; }
        }

        string IMigrationMetadata.Target
        {
            get { return Resources.GetString("Target"); }
        }
    }
}
"@
Set-Content -Path "$migrationDir\${timestamp}_${className}.Designer.cs" -Value $designerContent -Encoding UTF8
Write-Output "Created: ${timestamp}_${className}.Designer.cs"

$resxContent = @"
<?xml version="1.0" encoding="utf-8"?>
<root>
  <xsd:schema id="root" xmlns="" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
    <xsd:import namespace="http://www.w3.org/XML/1998/namespace" />
    <xsd:element name="root" msdata:IsDataSet="true">
      <xsd:complexType>
        <xsd:choice maxOccurs="unbounded">
          <xsd:element name="metadata">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" />
              </xsd:sequence>
              <xsd:attribute name="name" use="required" type="xsd:string" />
              <xsd:attribute name="type" type="xsd:string" />
              <xsd:attribute name="mimetype" type="xsd:string" />
              <xsd:attribute ref="xml:space" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="assembly">
            <xsd:complexType>
              <xsd:attribute name="alias" type="xsd:string" />
              <xsd:attribute name="name" type="xsd:string" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="data">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
                <xsd:element name="comment" type="xsd:string" minOccurs="0" msdata:Ordinal="2" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" msdata:Ordinal="1" />
              <xsd:attribute name="type" type="xsd:string" msdata:Ordinal="3" />
              <xsd:attribute name="mimetype" type="xsd:string" msdata:Ordinal="4" />
              <xsd:attribute ref="xml:space" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="resheader">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" />
            </xsd:complexType>
          </xsd:element>
        </xsd:choice>
      </xsd:complexType>
    </xsd:element>
  </xsd:schema>
  <resheader name="resmimetype">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name="version">
    <value>2.0</value>
  </resheader>
  <resheader name="reader">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name="writer">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <data name="Target" xml:space="preserve">
    <value>$newBase64</value>
  </data>
  <data name="DefaultSchema" xml:space="preserve">
    <value>dbo</value>
  </data>
</root>
"@
Set-Content -Path "$migrationDir\${timestamp}_${className}.resx" -Value $resxContent -Encoding UTF8
Write-Output "Created: ${timestamp}_${className}.resx"

Write-Output "`nInitialCreate regenerada exitosamente con RecursoId."
Write-Output "Para aplicar: ejecuta Update-Database en Package Manager Console."
