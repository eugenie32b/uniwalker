namespace UniWalker
{
    public interface IUniWalkerParser
    {
        bool TryParse(string s, out UniWalker walker);
    }
}
