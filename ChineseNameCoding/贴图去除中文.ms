fn Regex expression=
(
    rgx = dotNetObject "System.Text.RegularExpressions.Regex" expression
    return rgx
)

fn f_URLEncode _STR =
(
    reg = Regex "[\u4e00-\u9fa5]"
    bResult  = reg.IsMatch _STR
    if bResult == true then 
    (
        dotnet.loadassembly "System.Web"
        local httpUtility = dotnetClass "System.Web.HttpUtility"
        local urlEncodedString = httpUtility.UrlEncode _STR

    )
    else
    (
        return  _STR 
    )
 )

fn f_Convet2Pinyin _STR =
(
    reg = Regex "[\u4e00-\u9fa5]"
    bResult  = reg.IsMatch _STR
    if bResult == true then 
    (
          dotnet.loadassembly "./Chinese2Pinyin.dll"
          local  obj = dotnetClass "Chinese2Pinyin.PinyinHelper"
          if obj == undefined then 
          (
              return _STR
          )
          else
          (
              local  rst = obj.GetPinyin _STR
              return rst
          )
    )
    else
    (
        return _STR
    )
)

fn f_ChangeTextureName mat index =
(
    tex = getSubTexmap  mat index
    if tex == undefined then
    (
        print mat.name +"missing texture" 
    )
    else
    (
        local filename = filenameFromPath  tex.filename
        local dir = getFilenamePath tex.filename

        texfilename= f_Convet2Pinyin filename
        tex.filename =dir+ texfilename

        print filename
        print texfilename
    )
)

fn f_ChangeMateialName mat = 
(
    tex = getSubTexmap  mat 2
    if tex == undefined then
    (
        print mat.name +"missing texture" 
    )
    else
    (
        local name = getFilenameFile  tex.filename
        mat.name = name
        print mat.name
    )
)

fn f_ConvertChineseName obj =
(
    if obj.material !=undefined then
    (
        local num = getNumSubMtls obj.material  
        if num ==0 then
        (
             tex = getSubTexmap  obj.material  2
        )
        else
        (
            for mat in obj.material do
            (
                f_ChangeTextureName mat 2
            )
        )
    )
)

fn f_ConvertMaterialName obj =
(
    if obj.material !=undefined then
    (
        local num = getNumSubMtls obj.material  
        if num ==0 then
        (
        )
        else
        (
            for mat in obj.material do
            (
                f_ChangeMateialName mat
            )
        )
    )
)

rollout win "中文处理" width:176 height:300
(
    button 'btnSelection' "全选" pos:[28,10] width:120 height:40 align:#left
    button 'btnConvert' "贴图去除中文" pos:[28,60] width:120 height:40 align:#left
    button 'btnRename' "材质用贴图命名" pos:[28,110] width:120 height:40 align:#left
    
    on btnSelection pressed do
    (
        select $*
    )
    on btnConvert pressed  do
    (
        for obj in selection do   
        (
            f_ConvertChineseName obj
        )
    )
    on btnRename pressed  do
    (
        for obj in selection do   
        (
            f_ConvertMaterialName obj
        )
    )
)

createDialog win 176 300