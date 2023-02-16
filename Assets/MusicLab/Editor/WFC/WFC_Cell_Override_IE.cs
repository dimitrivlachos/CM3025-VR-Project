using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WFC
{
    public interface WFC_Cell_Override_IE
    {
        void Override_single_cell(int row, int column, int[] new_options);
    }
}
