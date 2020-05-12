using System.Collections.Generic;
using System.Linq;

public struct DataTable
{
    public bool IsNotEmpty;

    private readonly Dictionary<string, int> fields;

    private List<object[]> rows;
    private List<object> row;
    private SortedList<int, string> fieldsIndex;

    public Row[] Rows { get; }

    public DataTable(List<string> fields, List<object[]> rows)
    {
        this.fields = new Dictionary<string, int>();
        int count = 0;
        foreach (var item in fields)
        {
            this.fields.Add(item, count);
            count++;
        }
        Rows = new Row[rows.Count];
        for (int i = 0; i < rows.Count; i++)
            Rows[i] = new Row(rows[i], this.fields);

        this.rows = new List<object[]>();
        row = new List<object>();
        fieldsIndex = new SortedList<int, string>();

        IsNotEmpty = rows.Count!=0;
    }

    public DataTable(string[] fields, List<object[]> rows)
    {
        this.fields = new Dictionary<string, int>();
        int count = 0;
        foreach (var item in fields)
        {
            this.fields.Add(item, count);
            count++;
        }
        Rows = new Row[rows.Count];
        for (int i = 0; i < rows.Count; i++)
            Rows[i] = new Row(rows[i], this.fields);

        this.rows = new List<object[]>();
        row = new List<object>();
        fieldsIndex = new SortedList<int, string>();

        IsNotEmpty = rows.Count != 0;
    }

    public struct Row
    {
        public object[] Cells { get; }
        private Dictionary<string, int> fields;

        public Row(object[] cells, Dictionary<string, int> fields)
        {
            Cells = cells;
            this.fields = fields;
        }

        public object this[int index]
        {
            get
            {
                if (index < 0 || index >= Cells.Length)
                    return null;
                return Cells[index];
            }
        }

        public object this[string field]
        {
            get
            {
                if (fields.TryGetValue(field, out int value))
                    return Cells[value];
                else
                    return null;
            }
        }
    }

    public IEnumerable<string> Fields
    {
        get { return fields.Keys; }
    }

    public int GetFieldIndex(string field)
    {
        return fields[field];
    }

    public DataTable Select(string[] fields = null, (string, object)[] where = null)
    {
        rows.Clear();
        fieldsIndex.Clear();

        if (fields != null)
            foreach (var item in fields)
                fieldsIndex.Add(GetFieldIndex(item), item);
        else
        {
            int count = 0;
            foreach (var item in Fields)
            {
                fieldsIndex.Add(count, item);
                count++;
            }
        }
        
        foreach (var item in Rows.Where(x =>
        {
            if (where != null)
                foreach (var i in where)
                    if (!x[i.Item1].Equals(i.Item2))
                        return false;
            return true;
        }))
        {
            row.Clear();
            foreach (var i in fieldsIndex.Values)
                row.Add(item[i]);
            if(row.Count!=0)
                rows.Add(row.ToArray());
        }
        return new DataTable(fieldsIndex.Values.ToArray(), rows);
    }
}
