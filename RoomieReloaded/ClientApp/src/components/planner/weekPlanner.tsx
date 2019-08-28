import { Planner } from "./base/planner";
import { getDayOfWeek, Days } from './utility/dateTimeHelper';
import { IHasDateRange } from './base/plannerTypes';
import { MainHeaderDefinesViewPartRenderer, IHeaderRenderer } from './base/renderer/headerRenderer';

export class WeekPlanner extends Planner {

    getDaysToRender():number{
        return 5;
    }

    getViewRange():IHasDateRange{
        const {date} = this.props;

        const daysToRender = this.getDaysToRender();

        const start = getDayOfWeek(Days.MONDAY, date);
        const end = start.clone().add(daysToRender, 'days');
        return {
            start,
            end
        };
    }

    getShownViewRange = ():IHasDateRange =>
    {
        const{
            startHour,
            endHour
        } = this.props;
        
        const lastDayHoursSubtract = 24 - endHour;
        const viewRange = this.getViewRange();

        return {
            start:viewRange.start.add(startHour, 'hours'),
            end:viewRange.end.subtract(lastDayHoursSubtract, 'hours')
        };
    }

    getHoursShownPerColumn = ():number =>
    {
        const{
            startHour,
            endHour
        } = this.props;
        return endHour - startHour;
    }

    getDragIntervalInMinutes():number
    {
        return 15;
    }

    getViewPartRanges():IHasDateRange[]{
        const { startHour, endHour} = this.props;
        
        const viewRange = this.getViewRange();

        const dayRanges : IHasDateRange[] = [];
        const daysToRender = this.getDaysToRender();

        for (let index = 0; index < daysToRender; index++) {

            const dayStart = viewRange.start.clone().add(index, 'days');

            const startDateTime = dayStart.clone().add(startHour, 'hours');
            const endDateTime = dayStart.clone().add(endHour, 'hours');
            
            const dayRange : IHasDateRange = {start:startDateTime, end:endDateTime };
                        
            dayRanges.push(dayRange);
        }

        return dayRanges;
    }

    getHeaderValues():string[] {
        const {date} = this.props;
        
        const daysToRender = this.getDaysToRender();
        
        const weekStart = getDayOfWeek(Days.MONDAY, date); 

        const headerStrings : string[] = [];

        for (let index = 0; index < daysToRender; index++) {
            const currentDay = weekStart.clone().add(index, 'days');
            headerStrings.push(currentDay.format('dddd, DD.MM.'))
        }

        return headerStrings;
    }

    getSubHeaderValues():string[]    
    {
        const{
            startHour,
            endHour
        } = this.props;
        
        return [
            this.getTimeString(startHour),
            this.getTimeString(endHour === 0 ? 24 : endHour)
        ]
    }

    getSubheadersDefineViewParts():boolean{
        return false;
    }

    getHeaderRenderer(): IHeaderRenderer{
        return new MainHeaderDefinesViewPartRenderer();
    }
}