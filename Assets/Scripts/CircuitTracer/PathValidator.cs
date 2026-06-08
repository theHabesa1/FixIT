using System.Collections.Generic;

public static class PathValidator
{
    // Checks that the drawn path forms a valid chain from source to dest.
    public static bool ValidatePath(List<Node> path, Node source, Node dest)
    {
        if (path.Count < 2) return false;
        if (path[0] != source || path[path.Count - 1] != dest) return false;
        for (int i = 0; i < path.Count - 1; i++)
            if (!path[i].neighbours.Contains(path[i + 1])) return false;
        return true;
    }
}
