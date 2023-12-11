----------------------UDF Value Blocking Starts------------------
IF (:transaction_type=(n'A') and :object_type='67'  ) THEN
select (
Select 1 from OWTR T0 Where T0."DocEntry"=:list_of_cols_val_tab_del and (T0."U_AT_GIEntry" is not null or T0."U_AT_InvEntry" is not null)
)
INTO temp_var_0 FROM DUMMY;
IF :temp_var_0 > 0
THEN error := 10012;
error_message := 'Value Found in Goods Issue Entry UDF or Inventory Transfer Entry UDF. Please remove...';
End IF;
End IF;

IF (:transaction_type=(n'A') and :object_type='60' ) THEN
select (
Select 1 from OIGE T0 Where T0."DocEntry"=:list_of_cols_val_tab_del and (T0."U_AT_GIEntry" is not null or T0."U_AT_InvEntry" is not null)
)
INTO temp_var_0 FROM DUMMY;
IF :temp_var_0 > 0
THEN error := 10012;
error_message := 'Value Found in Goods Issue Entry UDF or Inventory Transfer Entry UDF. Please remove...';
End IF;
End IF;

--------------------------UDF Value Blocking Ends-----------------------

-------------------------Query Report Updated in 21st Aug 2023--------------------------------------------

Select distinct T0."DocNum" "Inventory Transfer #",T0."DocEntry" "Inventory Transfer DocEntry",T0."DocDate",T0."ToWhsCode" "To Warehouse",
T1."U_GIEntry" "Goods Issue Entry",T1."U_ErrId" "Error Code",Cast(T1."U_ErrDesc" as varchar) "Error Description",T1."U_Flag" "Flag",
T1."U_Status" "Status"
from OWTR T0 Left Join "@ATPL_ITGI" T1 On T0."DocEntry"=T1."U_BaseEntry" 
Left Join WTR1 T2 On T0."DocEntry"=T2."DocEntry"
Where T0."CANCELED"='N' and T0."DocDate">='20230814' and T0."ToWhsCode" in ('COP','COPQC') and T2."Quantity">0
and T0."DocDate" between [%0] and [%1]
Order By T0."DocDate" desc



--------------------------Old-----------------------------------------
Select T0."DocNum" "Inventory Transfer #",T0."DocEntry" "Inventory Transfer DocEntry",T0."DocDate",T0."ToWhsCode" "To Warehouse",
T1."U_GIEntry" "Goods Issue Entry",T1."U_ErrId" "Error Code",T1."U_ErrDesc" "Error Description",T1."U_Flag" "Flag",T1."U_Status" "Status"
from OWTR T0 Left Join "@ATPL_ITGI" T1 On T0."DocEntry"=T1."U_BaseEntry" 
Where T0."CANCELED"='N' and T0."DocDate">='20230814' and T0."ToWhsCode" in ('COP','COPQC')
and T0."DocDate" between [%0] and [%1]
Order By T0."DocDate",T0."DocEntry" desc
--------------------------------------------------------------------------------------------