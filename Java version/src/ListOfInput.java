public class ListOfInput {
    
    public Input head;
    public Input next;
    public int size;
    
    
    public ListOfInput(){}
    
    public ListOfInput(Input node) {
        if (head==null) {
            head=node;
        }
        size++;
    }
    
    public void addInput(Input node) {
        if(head!= null) {
            Input templ = head;
            while(templ.next != null) {
                templ = templ.next;
            }
            templ.next = node;
        }
        else {
            head = node;
        }
        size++;
    }
    
    public void clear() {
        head = null;
        size = 0;
    }
    
}
