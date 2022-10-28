using MDRCloudServices.DataLayer.Models;
using MDRCloudServices.DataLayer.SqlKata;
using MDRCloudServices.Enums;
using MDRCloudServices.Exceptions;
using MDRCloudServices.Interfaces;
using MDRCloudServices.Services.Handlers;
using MDRCloudServices.Services.Interfaces;
using MDRDB.Recordsets;
using MediatR;
using NPoco;
using SqlKata;

namespace MDRCloudServices.Services.Services;

/// <summary>
/// Service for manipulating recordsets
/// </summary>
/// <remarks>
/// Functions related to the table name of the recordset have been extracted
/// into a separate service to avoid circular dependencies between this and
/// the fields service. We have pass through methods here so that we don't
/// need to inject both the recordset and recordset table services into
/// the same controller.
/// </remarks>
public class RecordsetService : IRecordsetService
{
    private readonly IDatabase _db;
    private readonly IMediator _m;

    public RecordsetService(IDatabase db, IMediator m)
    {
        _db = db;
        _m = m;
    }

    /// <summary>Get Recordset by Id</summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task<Recordset> GetRecordsetAsync(int id)
    {
        var rs = await _db.SingleOrDefaultAsync<Recordset>(id);
        if (rs is null)
        {
            throw new NotFoundException($"Recordset Not Found: No recordset {id}");
        }

        if (!await _db.AnyAsync<Location>(x => x.Id == rs.Location))
        {
            throw new NotFoundException($"Recordset Not Locatable: Location {rs.Location} referred to by recordset {id} does not exist");
        }

        return rs;
    }

    /// <summary>Get location for recordset</summary>
    /// <param name="rs"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    public async Task<Location> GetLocationForRecordsetAsync(Recordset rs)
    {
        var location = await _db.SingleOrDefaultAsync<Location>(rs.Location);
        if (location == null) throw new NotFoundException("Storage Database Location Not Found");
        return location;
    }

    /// <summary>Get fields for recordset</summary>
    /// <param name="rs"></param>
    /// <param name="editMode"></param>
    /// <returns></returns>
    public async Task<List<IField>> GetFieldsForRecordset(Recordset rs, bool editMode)
    {
        if (rs.Mode == RecordsetMode.BINARY)
        {
            return new List<IField>()
                {
                    new Field()
                    {
                        Name = "File Name",
                        Type = "Text",
                        ColumnName ="FileName"
                    },
                    new Field()
                    {
                        Name = "File size",
                        Type = "FileSize",
                        ColumnName ="FileSize"
                    },
                    new Field()
                    {
                        Name = "File Type",
                        Type = "Text",
                        ColumnName ="FileType"
                    },
                    new Field()
                    {
                        Name = "Download Link",
                        Type = "Link",
                        ColumnName ="Links"
                    }
                };
        }

        var filterTypes = await _db.FetchAsync<FilterList>("");
        var query = new Query().FromRaw("Recordsets.Fields").Where("RecordsetId", rs.Id);

        if (!editMode)
            query.WhereIn("Type", new Query().FromRaw("Recordsets.FieldTypes").Select("Name").Where("IncludeInData", true));

        query.OrderBy("Sequence");

        var tempFields = await _db.FetchAsync<Field>(query);

        return tempFields
            .Select(f =>
            {
                f.ColumnName = (f.ColumnName ?? string.Empty).Replace("[", string.Empty).Replace("]", string.Empty);
                f.FilterTypes = filterTypes.Where(t => t.FieldType == f.Type).ToList();
                return f as IField;
            }).ToList();
    }

    /// <summary>Get all recordsets</summary>
    /// <returns></returns>
    public async Task<List<Recordset>> GetAllRecordsetsAsync()
    {
        var externalSchema = await _m.Send(new GetExternalSchemaQuery());
        return await _db.FetchAsync<Recordset>(
            $"SELECT r.* FROM Recordsets.Recordsets r INNER JOIN \"{externalSchema}\".LatestPublishedVersion v ON v.HoldingId = r.HoldingId " +
            "WHERE r.Draft = 0");
    }
}
