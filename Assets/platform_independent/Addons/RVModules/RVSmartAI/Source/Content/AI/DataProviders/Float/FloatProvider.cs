// Created by Ronis Vision. All rights reserved
// 13.10.2020.

namespace RVModules.RVSmartAI.Content.AI.DataProviders
{
    public abstract class FloatProvider : DataProvider<float>
    {
        #region Public methods

        public static implicit operator float(FloatProvider _floatProvider) => _floatProvider.GetData();
        public static implicit operator int(FloatProvider _floatProvider) => (int) _floatProvider.GetData();

        #endregion
    }
}