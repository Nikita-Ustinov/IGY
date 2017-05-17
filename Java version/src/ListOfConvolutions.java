
public class ListOfConvolutions {
  public Convolution head;
  public Convolution next;
  public int size;
  
  public ListOfConvolutions(Convolution node) {
    head=node;
    size++;
  }
  
  public void addConvolution(Convolution node) {
      if(head==null){
          head = node;
      }
      else {
          Convolution templ = head;
          while(templ.next!=null) {
              templ=templ.next;
          }
          templ.next= node;
      }
      size++;
  }
    
    
    
}
