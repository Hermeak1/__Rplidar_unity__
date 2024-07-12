using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Flags]
public enum KeyFlag
{
    /// <summary>
    /// 키 누름
    /// </summary>
    KE_DOWN = 0,
    /// <summary>
    /// 확장 키
    /// </summary>
    KE__EXTENDEDKEY = 1,
    /// <summary>
    /// 키 뗌
    /// </summary>
    KE_UP = 2
}

[Flags]
public enum MouseFlag
{
    /// <summary>
    /// 마우스 이동
    /// </summary>
    ME_MOVE = 1,
    /// <summary>
    /// 마우스 왼쪽 버튼 누름
    /// </summary>
    ME_LEFTDOWN = 2,
    /// <summary>
    /// 마우스 왼쪽 버튼 뗌
    /// </summary>
    ME_LEFTUP = 4,
    /// <summary>
    /// 마우스 오른쪽 버튼 누름
    /// </summary>
    ME_RIGHTDOWN = 8,
    /// <summary>
    /// 마우스 오른쪽 버튼 뗌
    /// </summary>
    ME_RIGHTUP = 0x10,
    /// <summary>
    /// 마우스 가운데 버튼 누름
    /// </summary>
    ME_MIDDLEDOWN = 0x20,
    /// <summary>
    /// 마우스 가운데 버튼 뗌
    /// </summary>
    ME_MIDDLEUP = 0x40,
    /// <summary>
    /// 마우스 휠
    /// </summary>
    ME_WHEEL = 0x800,
    /// <summary>
    /// 마우스 절대 이동
    /// </summary>
    ME_ABSOULUTE = 8000
}