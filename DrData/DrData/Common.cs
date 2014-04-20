﻿/*
  Common.cs -- common of the 'DrData' general purpose Data abstraction layer 1.0.1, January 3, 2014
 
  Copyright (c) 2013-2014 Kudryashov Andrey aka Dr
 
  This software is provided 'as-is', without any express or implied
  warranty. In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

      1. The origin of this software must not be misrepresented; you must not
      claim that you wrote the original software. If you use this software
      in a product, an acknowledgment in the product documentation would be
      appreciated but is not required.

      2. Altered source versions must be plainly marked as such, and must not be
      misrepresented as being the original software.

      3. This notice may not be removed or altered from any source distribution.

      Kudryashov Andrey <kudryashov.andrey@gmail.com>

 */

namespace DrOpen.DrCommon.DrData
{
    public enum ResolveConflict : int
    {
        /// <summary>
        /// in case of error throw exception
        /// </summary>
        THROW_EXCEPTION,
        /// <summary>
        /// do not update the existing object/value
        /// </summary>
        SKIP,
        /// <summary>
        /// overwrite existing object/value
        /// </summary>
        OVERWRITE
    }
}
