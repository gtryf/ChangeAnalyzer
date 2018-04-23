namespace ChangeAnalysis.Models
{
    public struct Change
    {
        public ChangeType ChangeType { get; }
        public string FileName { get; }

        public string OldContent { get; set; }
        public string NewContent { get; set; }
        
        public Change(string fileName, string oldContent, string newContent, ChangeType changeType)
        {
            this.FileName = fileName;
            this.OldContent = oldContent;
            this.NewContent = newContent;
            this.ChangeType = changeType;
        }

        public override string ToString()
        {
            switch (ChangeType)
            {
                case ChangeType.Addition:
                    return $"Added file {FileName}";
                case ChangeType.Deletion:
                    return $"Removed file {FileName}";
                case ChangeType.Modification:
                    return $"Modified file {FileName}";
                default:
                    return "Oops, unknown change type";
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Change)) return false;
            var other = (Change)obj;

            return
                this.ChangeType == other.ChangeType &&
                this.FileName == other.FileName &&
                this.OldContent == other.OldContent &&
                this.NewContent == other.NewContent;
        }

        public override int GetHashCode() => this.ToString().GetHashCode();
    }
}
