if exists("b:current_syntax")
  finish
endif

syn match iodineComment "#.*$"
syn match iodineEscape	contained +\\["\\'0abfnrtvx]+

syn match iodineNumber '\d\+'  
syn match iodineNumber '[-+]\d\+' 

syn match iodineNumber '\d\+\.\d*' 
syn match iodineNumber '[-+]\d\+\.\d*'

syn region iodineString start='"' end='"' contains=iodineEscape

syn keyword iodineKeyword if else for func class while break lambda self use return true false null foreach from in as try except raise super params interface enum yield switch case default
syn keyword iodineFunctions print println input map filter

syn region iodineBlock start="{" end="}" fold transparent contains=ALL


let b:current_syntax = "iodine"

hi def link iodineComment     Comment
hi def link iodineKeyword     Statement
hi def link iodineFunctions   Function
hi def link iodineString      Constant
hi def link iodineNumber      Constant
