-- Add SANBORN as item view type
if ( not exists ( select 1 from SobekCM_Item_Viewer_Types where ViewType = 'SANBORN' ))
begin
	
	insert into SobekCM_Item_Viewer_Types ( ViewType, [Order], DefaultView, MenuOrder )
	values ( 'SANBORN', 1, 0, 11 );

end;
GO


-- You can use this portion to change items in bulk to use the new downloads viewer, rather than the old
-- one.  Just change the collection code below to your collection code.
declare @collection_code varchar(20);
set @collection_code = 'SANBORN';

declare @collectionid int;
set @collectionid = ( select AggregationID from SobekCM_Item_Aggregation where Code=@collection_code );

declare @new_view_id int;
set @new_view_id = ( select ItemViewTypeID from SobekCM_Item_Viewer_Types where ViewType='SANBORN');

if (( coalesce(@collectionid, -1 ) > 0 ) and ( coalesce( @new_view_id, -1) > 0 ))
begin

	insert into SobekCM_Item_Viewers ( ItemID, ItemViewTypeID, Attribute, Label )
	select ItemID, @new_view_id, '', 'Index'
	from SobekCM_Item_Aggregation_Item_Link L
	where L.AggregationID = @collectionid
	  and not exists ( select 1 from SobekCM_Item_Viewers V where V.ItemID=L.ItemID and V.ItemViewTypeID=@new_view_id);
end
else
begin
	print 'Error trying to assign the new view to the collection.. some value is null';
	print '     Collection ID = ' + cast(coalesce(@collectionid, -1) as varchar(5));
	print '     New View ID = ' + cast(coalesce(@new_view_id, -1) as varchar(5));
end;
GO


