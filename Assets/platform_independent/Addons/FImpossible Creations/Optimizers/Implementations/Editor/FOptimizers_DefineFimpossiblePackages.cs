using UnityEditor;

namespace FIMSpace.FEditor
{
    /// <summary>
    /// FM: Class to help define if adressables are imported
    /// </summary>
    [InitializeOnLoad]
    sealed class FOptimizers_DefineFimpossiblePackages
    {
        const string define1 = "OPT_TAILANIMATOR_IMPORTED";
        const string define2 = "OPT_SPINEANIMATOR_IMPORTED";
        const string define3 = "OPT_LOOKANIMATOR_IMPORTED";

        static FOptimizers_DefineFimpossiblePackages()
        {
            if (FDefinesCompilation.GetTypesInNamespace("FIMSpace.FTail", "").Count > 1)
                FDefinesCompilation.SetDefine(define1);
            else
                FDefinesCompilation.RemoveDefine(define1);

            if (FDefinesCompilation.GetTypesInNamespace("FIMSpace.FSpine", "").Count > 1)
                FDefinesCompilation.SetDefine(define2);
            else
                FDefinesCompilation.RemoveDefine(define2);


            if (FDefinesCompilation.GetTypesInNamespace("FIMSpace.FLook", "").Count > 1)
                FDefinesCompilation.SetDefine(define3);
            else
                FDefinesCompilation.RemoveDefine(define3);
        }
    }
}
