```
Dot:
Color, State, isFirst
bool Stasis => vel < gate                  #速度小于阈值，就说不在动

if vel < gate && state == static => lineup #速度小于阈值，且是空心的，就请求连线
if vel > gate && state == lined => unline. #速度大于阈值，且是实心的，就请求断连


Manager:
LineUp(Dot target)
{
    if target.State == lined: return    #已经连线了，直接返回

    if target isFirst:
        if queue.count == 0:            #刚发出去，不连线，加到queue
            queue.add(target)
            return
        else:                           #中心点要连线，则对于中心点底下所有的点都递归一次连线
            foreach queue dot:
                LineUp(dot)
    
    else:
        if first not Stasis: return     #如果中心点在动，那就连线不了。接下来情况都是中心点不动的。
        
        if queue.count == 1:            #如果只有一个中心点，首先将两个点都设为lined
            first.State = Lined         #因为这个函数执行完之后，这个点就设为lined了，所以不会执行第二遍。
            target.State = Lined        #把此点和中心点都设为lined，并且加入currentline
            first.Currentlines += line
            target.Currentline = line
            await firstAnim
            await lineAnim
            await targetAnim
        
        else:
            if first.State == Static     #有其他点，然后检查中心点是否已连线。
            target.State = Lined         #中心点没连线，也只可能是中心点返回的


}
