----------------Expense Account Code UDF in Item Master is null blocking the Inventory transfer-------------------
IF (:transaction_type=(n'A') and :object_type='67' ) THEN
select (
Select distinct 1 from OITM T0 Where T0."ItemCode" in (Select A."ItemCode" from WTR1 A Left Join OWTR B On A."DocEntry"=B."DocEntry"
Where A."DocEntry"=:list_of_cols_val_tab_del and A."WhsCode" in ('F & O','R&D') ) and T0."U_ExpAcc" is null
)
INTO temp_var_0 FROM DUMMY;
IF :temp_var_0 > 0
THEN error := 10013;
error_message := 'Account Code is not defined for the Items: '|| 
(Select STRING_AGG(T0."ItemCode",' , ') from OITM T0 Where T0."ItemCode" in (Select A."ItemCode" from WTR1 A Left Join OWTR B On A."DocEntry"=B."DocEntry"
Where A."DocEntry"=:list_of_cols_val_tab_del and A."WhsCode" in ('F & O','R&D') ) and T0."U_ExpAcc" is null);

End IF;
End IF;
---------------------------------------------------------------------------------------------------------------------------------------