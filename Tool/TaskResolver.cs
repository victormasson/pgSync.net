// namespace PgSync;
// public class TaskResolver
// {
//     private readonly string[] args;
//     private readonly Options opts;
//     private readonly DataSource source;
//     private readonly DataSource destination;
//     private readonly Dictionary<string, List<string[]>> groups;
//     private readonly string firstSchema;
//     private readonly List<string> notes;

//     public TaskResolver(string[] args, Options opts, DataSource source, DataSource destination, Dictionary<string, List<string[]>> groups, string firstSchema)
//     {
//         this.args = args;
//         this.opts = opts;
//         this.source = source;
//         this.destination = destination;
//         this.groups = groups;
//         this.firstSchema = firstSchema;
//         this.notes = new List<string>();
//     }

//     public List<Task> ResolveTasks()
//     {
//         List<Task> tasks = new List<Task>();

//         // get lists from args
//         var (groups, tables) = ProcessArgs();

//         // expand groups into tasks
//         foreach (var group in groups)
//         {
//             tasks.AddRange(GroupToTasks(group));
//         }

//         // expand tables into tasks
//         foreach (var table in tables)
//         {
//             tasks.AddRange(TableToTasks(table));
//         }

//         // get default if none given
//         if (!opts.Groups && !opts.Tables && args.Length == 0)
//         {
//             tasks.AddRange(DefaultTasks());
//         }

//         // resolve any tables that need it
//         foreach (var task in tasks)
//         {
//             task.Table = FullyResolve(task.Table);
//         }

//         return tasks;
//     }

//     public bool IsGroup(string group)
//     {
//         return groups.ContainsKey(group);
//     }

//     // Rest of the methods translation
//     // ...
// }
// }