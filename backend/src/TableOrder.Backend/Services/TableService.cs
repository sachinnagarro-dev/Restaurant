using Microsoft.EntityFrameworkCore;
using TableOrder.Backend.Data;
using TableOrder.Backend.DTOs;
using TableOrder.Backend.Models;

namespace TableOrder.Backend.Services;

public interface ITableService
{
    Task<IEnumerable<TableResponse>> GetTablesAsync();
    Task<TableResponse?> GetTableByIdAsync(int id);
    Task<TableResponse?> GetTableByNumberAsync(int number);
    Task<TableResponse?> UpdateTableStatusAsync(int id, TableStatus status);
}

public class TableService : ITableService
{
    private readonly TableOrderDbContext _context;

    public TableService(TableOrderDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TableResponse>> GetTablesAsync()
    {
        var tables = await _context.Tables
            .OrderBy(t => t.Number)
            .ToListAsync();

        return tables.Select(MapToTableResponse);
    }

    public async Task<TableResponse?> GetTableByIdAsync(int id)
    {
        var table = await _context.Tables.FindAsync(id);
        return table == null ? null : MapToTableResponse(table);
    }

    public async Task<TableResponse?> GetTableByNumberAsync(int number)
    {
        var table = await _context.Tables
            .FirstOrDefaultAsync(t => t.Number == number);
        
        return table == null ? null : MapToTableResponse(table);
    }

    public async Task<TableResponse?> UpdateTableStatusAsync(int id, TableStatus status)
    {
        var table = await _context.Tables.FindAsync(id);
        if (table == null)
            return null;

        table.Status = status;
        table.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToTableResponse(table);
    }

    private static TableResponse MapToTableResponse(Table table)
    {
        return new TableResponse
        {
            Id = table.Id,
            Number = table.Number,
            Capacity = table.Capacity,
            Status = table.Status.ToString()
        };
    }
}
