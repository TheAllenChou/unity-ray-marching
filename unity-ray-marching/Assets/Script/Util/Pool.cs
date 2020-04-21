/******************************************************************************/
/*
  Project - Unity Ray Marching
            https://github.com/TheAllenChou/unity-ray-marching

  Author  - Ming-Lun "Allen" Chou
  Web     - http://AllenChou.net
  Twitter - @TheAllenChou

*/
/******************************************************************************/

using System.Collections.Generic;

public class Pool<T> where T : class
{
  private static List<T> s_aFreeObj = new List<T>();

  public static T Take()
  {
    if (s_aFreeObj.Count == 0)
      return default(T);

    T obj = s_aFreeObj[s_aFreeObj.Count - 1];
    s_aFreeObj.RemoveAt(s_aFreeObj.Count - 1);

    return obj;
  }

  public static void Store(T obj)
  {
    s_aFreeObj.Add(obj);
  }
}
