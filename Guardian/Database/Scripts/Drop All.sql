/*************************************************************************************************************************
*
*	File:			drop all.sql
*	Description:	This query drops every known user object from the database.  Used to make clean builds without
*					dropping and rebuilding the database.
*					Fluid Trade  Copyright ©  2002 - All Rights Reserved
*
*************************************************************************************************************************/

/* Set the Environment */

set nocount on
set quoted_identifier on

/* Declarations */

declare @parent_name sysname
declare @constraint_name sysname
declare @object_name sysname
declare @object_type "Char"(2)
declare @object_type_text "varchar"(32)

	/* TheIdea of this function is to remove the table, which means that any contraints that reference this table need to
	be dropped.  This cursor will use the 'Sysreferences' table to locate those contraints.  Note that the foreign keys must
	be deleted before the primary keys (because the foreign keys must reference the primary key), so we have to drop them
	first.  The 'Priority' constant allows us to order this statement do purge the foreign constraints first. */

	declare "ForeignKeyCursor" cursor local
	for
		select quotename(sysobjects1.name, '""'), quotename(sysobjects2.name, '""')
		from sysobjects sysobjects1, sysobjects sysobjects2, sysreferences
		where
			sysobjects1.id = sysreferences.fkeyid and
			sysreferences.constid = sysobjects2.id
	for read only

	/* Open the cursor that checks for foreign keys that reference the table being dropped. */
	
	open "ForeignKeyCursor"

	/* Cycle through all the contraints that reference the table being dropped and remove the contraint. */
	
	fetch next from "ForeignKeyCursor" into @parent_name, @constraint_name
	while (@@FETCH_STATUS = 0)
	begin
		execute ('Alter table ' + @parent_name + ' drop constraint ' + @constraint_name)
		print convert(varchar, getdate(), 120) + 'Z Constraint: ' + @parent_name + '.' + @constraint_name + ', Dropped'
		fetch next from "ForeignKeyCursor" into @parent_name, @constraint_name
	end

	/* Close down the cursor and free the resources. */

	close "ForeignKeyCursor"
	deallocate "ForeignKeyCursor"
	
	/* Now that the foreign keys dependancies have been removed, we can remove the user object. */
	
	declare "UserObjectCursor" cursor local
	for
		select quotename(sysobjects.name, '""'), type
		from sysobjects where type in ('U', 'P', 'TR', 'FN', 'V')
		and name not like 'dt_%' and name not like 'sys%'
	for read only

	/* Open the cursor that checks for foreign keys that reference the table being dropped. */
	
	open "UserObjectCursor"

	/* Cycle through all the contraints that reference the table being dropped and remove the contraint. */
	
	fetch next from "UserObjectCursor" into @object_name, @object_type
	while (@@FETCH_STATUS = 0)
	begin

		select @object_type_text = 
			case @object_type
				when 'U' then 'Table'
				when 'P' then 'Procedure'
				when 'TR' then 'Trigger'
				when 'FN' then 'Function'
				when 'V' then 'View'
			end

		execute ('Drop ' + @object_type_text + ' ' + @object_name)
		print convert(varchar, getdate(), 120) + 'Z ' + @object_type_text + ': ' + @object_name + ', Dropped'
		fetch next from "UserObjectCursor" into @object_name, @object_type

	end

	/* Close down the cursor and free the resources. */

	close "UserObjectCursor"
	deallocate "UserObjectCursor"

go
