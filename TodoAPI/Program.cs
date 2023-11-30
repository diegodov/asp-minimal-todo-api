using Microsoft.EntityFrameworkCore;

public class Todo {
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsCompleted { get; set; }
}

public class TodoDb : DbContext {
    public TodoDb(DbContextOptions<TodoDb> options) : base(options) { }
    public DbSet<Todo> Todos { get; set; } = null!;
}

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.MapGet("/todo", FindAll);
        app.MapGet("/todo/{id}", FindById);
        app.MapPost("/todo", Create);
        app.MapPut("/todo/{id}", Update);
        app.MapDelete("/todo/{id}", Delete);

        app.Run();
    }

    static async Task<IResult> FindAll(TodoDb db)
    {
        return TypedResults.Ok(await db.Todos.ToArrayAsync());
    }

    static async Task<IResult> FindAllCompleted(TodoDb db)
    {
        return TypedResults.Ok(await db.Todos.Where(t => t.IsComplete).ToListAsync());
    }

    static async Task<IResult> FindById(int id, TodoDb db)
    {
        return await db.Todos.FindAsync(id)
            is Todo todo
                ? TypedResults.Ok(todo)
                : TypedResults.NotFound();
    }

    static async Task<IResult> Create(Todo todo, TodoDb db)
    {
        db.Todos.Add(todo);
        await db.SaveChangesAsync();

        return TypedResults.Created($"/todoitems/{todo.Id}", todo);
    }

    static async Task<IResult> Update(int id, Todo inputTodo, TodoDb db)
    {
        var todo = await db.Todos.FindAsync(id);

        if (todo is null) return TypedResults.NotFound();

        todo.Name = inputTodo.Name;
        todo.IsComplete = inputTodo.IsComplete;

        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    static async Task<IResult> Delete(int id, TodoDb db)
    {
        if (await db.Todos.FindAsync(id) is Todo todo)
        {
            db.Todos.Remove(todo);
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        }

        return TypedResults.NotFound();
    }
}
