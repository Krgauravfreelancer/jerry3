<style>
    h1{
        background: #97C3F2;
        margin-top:50px;
    }
</style>

## _**Legends**_

    [ ] Pending Task/section
    [x] Done Task/Section

# [X] Create dll

Right now the 'ShapeBar' and 'Designer' are part on the same project as a in-project components. We are intend to add one more component to this control. For easy integration it is good to move all these components into its own user control and use its dll into the wrapper and so in the main project.

# [X] Shape Bar

## [X] Add Shape on the the 'Shape Bar'

Add the following shapes on the 'Shape Bar' component.
For each shape a method is required to be added that enables the drag of that shape and provides data to be dropped.

    See the already added shapes.

-   [x] Circle
-   [x] Rectangle
-   [x] Arrow
-   [x] Chat bubble
-   [x] Line
-   [x] Label/text

# [ ] Designer

## [X] Drag to move

A user can drag already added shape on the designer and move it around using mouse.

## [x] Resize Feature

A user can change the height and width of the shape on the designer.

-   [ ] When the mouse hover above the border of the mouse pointer to resize pointer.
-   [ ] If user clicks and moves the mouse change the height/width accordingly.
-   [ ] <div style="color:red; display:inline" > CAUTION! </div>: This also involves "mouse moves" event that is already been used in the "Dra to Move" feature.

## [X] Select Element

When a user just clicks on an shape select that UI Element. We need to pass the reference of the selected element to the "Properties Window" so its _'properties'_ can be set.

## [ ] Export as image

Implement a public method which can be invoke from the main program. This method will convert the complete canvas to an png/bmp image.  
The exported image will be used to do overlay on the final video.

# [ ] Properties Window

Implement an _Properties Window_ that can display current properties of a shape (UI Element) on a designer canvas.  
This control can set the following properties.

-   [x] Color
-   [x] Size
    -   [x] Height
    -   [x] Width
-   [x] Location
    -   [x] x
    -   [x] y
-   [ ] Margin
-   [x] Hidden
-   [x] Rotate degree
-   [x] Multiline (only for Label/text)
-   [x] Text (only for label/text)

<!-- # [ ] Outline Component -->

-   [] Output XAML
-   [] Covering component
-   [] View Only Component
    -   []
